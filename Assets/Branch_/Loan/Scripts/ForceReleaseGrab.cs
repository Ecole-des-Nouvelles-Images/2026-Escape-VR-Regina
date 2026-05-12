using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ForceReleaseGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    public void ForceRelease()
    {
        if (grabInteractable == null)
            return;

        // Si l'objet est grab
        if (grabInteractable.isSelected)
        {
            // Interactor qui tient l'objet
            var interactor = grabInteractable.firstInteractorSelecting;

            // Force le release
            grabInteractable.interactionManager.SelectExit(
                interactor,
                grabInteractable
            );
        }
    }
}
