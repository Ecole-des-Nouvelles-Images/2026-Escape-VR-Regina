using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRPokeInteractor))]
public class PokeEventHandler : MonoBehaviour
{
    private XRPokeInteractor _pokeInteractor;

    private void Awake()
    {
        _pokeInteractor = GetComponent<XRPokeInteractor>();
    }

    private void OnEnable()
    {
        if (_pokeInteractor != null)
        {
            _pokeInteractor.selectEntered.AddListener(OnPoke);
        }
    }

    private void OnDisable()
    {
        if (_pokeInteractor != null)
        {
            _pokeInteractor.selectEntered.RemoveListener(OnPoke);
        }
    }

    private void OnPoke(SelectEnterEventArgs args)
    {
        // Try to get Pin component from the poked object
        Pin pin = args.interactableObject.transform.GetComponent<Pin>();
        
        if (pin != null)
        {
            pin.OnPoked();
        }
        else
        {
            // Check if pin is on a parent object
            pin = args.interactableObject.transform.GetComponentInParent<Pin>();
            if (pin != null)
            {
                pin.OnPoked();
            }
        }
    }
}