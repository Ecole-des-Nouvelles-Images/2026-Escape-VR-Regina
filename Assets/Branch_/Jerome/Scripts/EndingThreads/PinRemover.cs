using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PinRemover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThreadManager _threadManager;
    [SerializeField] private XRPokeInteractor _pokeInteractor; // Assign from hand
    
    [Header("Visual")]
    [SerializeField] private Material _removeHighlightMaterial;
    [SerializeField] private float _highlightDuration = 0.2f;

    private void OnEnable()
    {
        if (_pokeInteractor != null)
        {
            _pokeInteractor.selectEntered.AddListener(OnPokeEnter);
        }
    }

    private void OnDisable()
    {
        if (_pokeInteractor != null)
        {
            _pokeInteractor.selectEntered.RemoveListener(OnPokeEnter);
        }
    }

    private void OnPokeEnter(SelectEnterEventArgs args)
    {
        // Check if poked object is a pin
        Pin pin = args.interactableObject.transform.GetComponent<Pin>();
        
        if (pin != null && pin.IsInChain())
        {
            // Visual feedback
            StartCoroutine(FlashPin(pin));
            
            // Remove the pin
            _threadManager.RemovePin(pin);
        }
    }

    private System.Collections.IEnumerator FlashPin(Pin pin)
    {
        Renderer renderer = pin.GetComponent<Renderer>();
        if (renderer != null && _removeHighlightMaterial != null)
        {
            Material originalMaterial = renderer.material;
            renderer.material = _removeHighlightMaterial;
            yield return new WaitForSeconds(_highlightDuration);
            renderer.material = originalMaterial;
        }
    }
}