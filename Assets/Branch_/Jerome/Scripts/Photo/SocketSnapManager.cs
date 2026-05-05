using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketSnapHandler : MonoBehaviour
{
    public GameObject expectedPiece;
    private XRSocketInteractor socket;
    private bool isOccupied = false;
    private AssemblyManager manager;
    
    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
       // manager = FindObjectOfType<AssemblyManager>();
        socket.selectEntered.AddListener(OnSnapped);
    }

    void OnSnapped(SelectEnterEventArgs args)
    {
        Debug.Log("Viewport " + XRSettings.renderViewportScale);
        if (isOccupied) return;
        
        GameObject snappedPiece = args.interactableObject.transform.gameObject;
        
        // Wrong piece - immediately eject
        if (snappedPiece != expectedPiece)
        {
            // Force release the wrong piece
            socket.interactionManager.SelectExit(args.interactorObject, args.interactableObject);
            Rigidbody rb = args.interactableObject.transform.GetComponent<Rigidbody>();
            rb.AddForce(args.interactableObject.transform.forward * 1, ForceMode.Impulse);
            return;
        }
        
        // Correct piece - accept it
        isOccupied = true;
        snappedPiece.GetComponent<XRGrabInteractable>().enabled = false;
        socket.enabled = false;
       // manager.PiecePlaced();
    }
}