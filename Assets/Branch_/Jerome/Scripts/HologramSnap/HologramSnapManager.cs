using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HologramSnapManager : MonoBehaviour
{
    [SerializeField] private GameObject _expectedPiece;
    private GameObject _snappedPiece;
    private XRSocketInteractor _socket;
    private bool _isOccupied = false;
    private FinalPuzzleManager _manager;
    
    private void Start()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _manager = FindFirstObjectByType<FinalPuzzleManager>();
        
        _socket.selectEntered.AddListener(OnSnapped);
    }
    
    private void OnSnapped(SelectEnterEventArgs args)
    {
        if (_isOccupied) return;
        
        GameObject snappedPiece = args.interactableObject.transform.gameObject;
        
        // Check if it's the expected piece
        bool isCorrect = snappedPiece == _expectedPiece;
        
        if (!isCorrect)
        {
            // Wrong piece - reject it and notify manager
            // _socket.interactionManager.SelectExit(args.interactorObject, args.interactableObject);
            
            if (_manager != null)
                _manager.OnWrongPiecePlaced(snappedPiece, this);
            
            return;
        }
        
        // Correct piece - accept it
        AcceptPiece(snappedPiece);
    }
    
    private void AcceptPiece(GameObject piece)
    {
        _snappedPiece = piece;
        _isOccupied = true;
        
        // Lock piece in place
        piece.transform.position = _socket.transform.position;
        piece.transform.rotation = _socket.transform.rotation;
        
        // Disable interaction
        XRGrabInteractable grabInteractable = piece.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
            grabInteractable.enabled = false;
        
        // Make kinematic
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
        
        // Disable socket
        _socket.enabled = false;
        
        // Notify manager
        if (_manager != null)
            _manager.OnCorrectPiecePlaced(piece, this);
    }
    
    public GameObject GetSnappedPiece()
    {
        return _snappedPiece;
    }
    
    public void ResetSocket()
    {
        _isOccupied = false;
        _snappedPiece = null;
        _socket.enabled = true;
    }
}