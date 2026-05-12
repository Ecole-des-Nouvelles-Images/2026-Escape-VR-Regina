using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketSnapHandler : MonoBehaviour
{
    public GameObject ExpectedPiece;
    private XRSocketInteractor _socket;
    private bool _isOccupied = false;
    private AssemblyManager _manager;
    private XRGrabInteractable _currentlyHoveredPiece;
    
    void Start()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _manager = FindFirstObjectByType<AssemblyManager>();
        
        _socket.selectEntered.AddListener(OnSnapped);
        _socket.hoverEntered.AddListener(OnHoverEntered);
        _socket.hoverExited.AddListener(OnHoverExited);
    }
    
    void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (_isOccupied) return;
        
        GameObject hoveredPiece = args.interactableObject.transform.gameObject;
        
        // Only track the correct piece
        if (hoveredPiece == ExpectedPiece)
        {
            _currentlyHoveredPiece = hoveredPiece.GetComponent<XRGrabInteractable>();
        }
    }
    
    void OnHoverExited(HoverExitEventArgs args)
    {
        if (_isOccupied) return;
        
        GameObject hoveredPiece = args.interactableObject.transform.gameObject;
        
        if (hoveredPiece == ExpectedPiece)
        {
            _currentlyHoveredPiece = null;
        }
    }
    
    void OnSnapped(SelectEnterEventArgs args)
    {
        if (_isOccupied) return;
        
        GameObject snappedPiece = args.interactableObject.transform.gameObject;
        
        // Wrong piece - eject it
        if (snappedPiece != ExpectedPiece)
        {
            _socket.interactionManager.SelectExit(args.interactorObject, args.interactableObject);
            Rigidbody rb = snappedPiece.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(_socket.transform.forward * 1, ForceMode.Impulse);
            return;
        }
        
        // Correct piece - accept it
        _isOccupied = true;
        
        // Force exact position
        snappedPiece.transform.position = _socket.transform.position;
        snappedPiece.transform.rotation = _socket.transform.rotation;
        
        snappedPiece.GetComponent<XRGrabInteractable>().enabled = false;
        
        Rigidbody rbCorrect = snappedPiece.GetComponent<Rigidbody>();
        if (rbCorrect != null)
            rbCorrect.isKinematic = true;
        
        _socket.enabled = false;
        _manager?.PiecePlaced();
    }
    
    void Update()
    {
        // If a correct piece is hovering AND player releases it
        if (_currentlyHoveredPiece != null && !_isOccupied)
        {
            // Check if the piece is no longer being held
            if (!_currentlyHoveredPiece.isSelected)
            {
                // Force it to snap into the socket
                ForceSnap();
            }
        }
    }
    
    void ForceSnap()
    {
        if (_currentlyHoveredPiece == null || _isOccupied) return;
        
        GameObject piece = _currentlyHoveredPiece.gameObject;
        
        // Manually force the socket to select it
        var interactor = _socket as IXRSelectInteractor;
        var interactable = _currentlyHoveredPiece as IXRSelectInteractable;
        
        if (interactor != null && interactable != null)
        {
            _socket.interactionManager.SelectEnter(interactor, interactable);
        }
    }
}