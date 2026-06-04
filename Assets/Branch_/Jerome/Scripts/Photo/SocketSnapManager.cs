using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

public class SocketSnapHandler : MonoBehaviour
{
    [SerializeField] private GameObject _expectedPiece;
    [SerializeField] private ParticleSystem _puzzleCompleted;
    private GameObject _savePiece;
    private XRSocketInteractor _socket;
    private bool _isOccupied = false;
    private AssemblyManager _manager;
    private XRGrabInteractable _currentlyHoveredPiece;
    private bool _hasSnappedCorrectly = false; // Track if this socket has completed its snap
    
    // Animation properties
    private bool _isAnimating = false;
   [SerializeField] private float _lerpDuration = 1f;

    private void Start()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _manager = FindFirstObjectByType<AssemblyManager>();
        
        _socket.selectEntered.AddListener(OnSnapped);
        _socket.hoverEntered.AddListener(OnHoverEntered);
        _socket.hoverExited.AddListener(OnHoverExited);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (_isOccupied || _hasSnappedCorrectly) return;
        
        GameObject hoveredPiece = args.interactableObject.transform.gameObject;
        
        // Only track the correct piece
        if (hoveredPiece == _expectedPiece)
        {
            _currentlyHoveredPiece = hoveredPiece.GetComponent<XRGrabInteractable>();
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (_isOccupied || _hasSnappedCorrectly) return;
        
        GameObject hoveredPiece = args.interactableObject.transform.gameObject;
        
        if (hoveredPiece == _expectedPiece)
        {
            _currentlyHoveredPiece = null;
        }
    }

    private void OnSnapped(SelectEnterEventArgs args)
    {
        if (_isOccupied || _hasSnappedCorrectly) return;
        
        GameObject snappedPiece = args.interactableObject.transform.gameObject;
        
        // Wrong piece - eject it
        if (snappedPiece != _expectedPiece)
        {
            _socket.interactionManager.SelectExit(args.interactorObject, args.interactableObject);
            Rigidbody rb = snappedPiece.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(_socket.transform.right * -2f, ForceMode.Impulse);
            return;
        }
        
        // Correct piece - accept it
        AcceptCorrectPiece(snappedPiece);
    }

    private void AcceptCorrectPiece(GameObject piece)
    {
        _savePiece = piece;
        _isOccupied = true;
        _hasSnappedCorrectly = true;
        
        // Force exact position
        piece.transform.position = _socket.transform.position;
        piece.transform.rotation = Quaternion.Euler(0, -90, 0);
        
        // Disable interaction
        XRGrabInteractable grabInteractable = piece.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
            grabInteractable.enabled = false;
        
        // Make kinematic to prevent physics interference
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
        // Turn off any colliders
        Collider cl = _savePiece.GetComponent<Collider>();
        if (cl != null)
            cl.enabled = false;
        
        // Optional: Hide or disable socket visuals
        _socket.enabled = false;
        
        // Notify the AssemblyManager that this piece has been snapped
        if (_manager != null)
        {
            _manager.PiecePlaced(piece, this);
        }
        else
        {
            Debug.LogError("AssemblyManager not found in scene!");
        }
        
        Debug.Log($"Piece {piece.name} successfully snapped to {gameObject.name}");
    }

    private void Update()
    {
        // If a correct piece is hovering AND player releases it
        if (_currentlyHoveredPiece != null && !_isOccupied && !_hasSnappedCorrectly)
        {
            // Check if the piece is no longer being held
            if (!_currentlyHoveredPiece.isSelected)
            {
                // Force it to snap into the socket
                ForceSnap();
            }
        }
    }

    private void ForceSnap()
    {
        if (!_currentlyHoveredPiece || _isOccupied || _hasSnappedCorrectly) return;
        
        GameObject piece = _currentlyHoveredPiece.gameObject;
        
        // Manually force the socket to select it
        if (_socket is IXRSelectInteractor interactor && _currentlyHoveredPiece is IXRSelectInteractable interactable)
        {
            _socket.interactionManager.SelectEnter(interactor, interactable);
        }
    }

    public void CompletedPuzzle()
    {
        if (_savePiece != null && !_isAnimating)
        {
            StartCoroutine(LerpToAttachTransform());
        }
    }
    
    private IEnumerator LerpToAttachTransform()
    {
        _isAnimating = true;
        
        // Get the attach transform or fallback to socket transform
        Transform targetTransform = _socket.attachTransform != null ? _socket.attachTransform : _socket.transform;
        Vector3 startPosition = _savePiece.transform.position;
        Quaternion startRotation = _savePiece.transform.rotation;
        Vector3 targetPosition = targetTransform.position;
        Quaternion targetRotation = targetTransform.rotation;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < _lerpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _lerpDuration;
            
            // Use SmoothStep for easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            _savePiece.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            _savePiece.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
            
            yield return null;
        }
        if (_puzzleCompleted) _puzzleCompleted.Play();
        // Ensure exact final position
        _savePiece.transform.position = targetPosition;
        _savePiece.transform.rotation = targetRotation;
        
        _isAnimating = false;
        Debug.Log($"Piece {_savePiece.name} animated to attach point");
    }
    
    // Method to reset the socket for testing/replay
    public void ResetSocket()
    {
        _isOccupied = false;
        _hasSnappedCorrectly = false;
        _currentlyHoveredPiece = null;
        _isAnimating = false;
        _socket.enabled = true;
    }
}