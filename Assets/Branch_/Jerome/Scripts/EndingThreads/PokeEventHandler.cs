using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRPokeInteractor))]
public class PokeEventHandler : MonoBehaviour
{
    private XRPokeInteractor pokeInteractor;
    
    void Awake()
    {
        pokeInteractor = GetComponent<XRPokeInteractor>();
    }
    
    void OnEnable()
    {
        if (pokeInteractor != null)
        {
            pokeInteractor.selectEntered.AddListener(OnPoke);
        }
    }
    
    void OnDisable()
    {
        if (pokeInteractor != null)
        {
            pokeInteractor.selectEntered.RemoveListener(OnPoke);
        }
    }
    
    void OnPoke(SelectEnterEventArgs args)
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