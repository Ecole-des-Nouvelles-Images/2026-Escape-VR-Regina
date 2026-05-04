using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketSnapHandler : MonoBehaviour
{
    public GameObject expectedPiece; // Which piece belongs here
    private XRSocketInteractor socket;
    [SerializeField] private bool isOccupied = false;
    private AssemblyManager manager;
    
    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        //manager = FindFirstObjectByType<AssemblyManager>();
        socket.selectEntered.AddListener(OnSnapped);
    }
    
    void OnSnapped(SelectEnterEventArgs args)
    {
        if (isOccupied) return;
        
        GameObject snappedPiece = args.interactableObject.transform.gameObject;
        
        // Check if it's the right piece
        if (snappedPiece != expectedPiece) return;
        
        isOccupied = true;
        
        // Disable grabbing on the piece
        snappedPiece.GetComponent<XRGrabInteractable>().enabled = false;
        
        // Disable this socket
        socket.enabled = false;
        
        // Notify manager
        //manager.PiecePlaced();
    }
}