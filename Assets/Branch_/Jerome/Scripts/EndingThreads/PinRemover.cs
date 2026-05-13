using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PinRemover : MonoBehaviour
{
    [Header("References")]
    public ThreadManager threadManager;
    public XRPokeInteractor pokeInteractor; // Assign from hand
    
    [Header("Visual")]
    public Material removeHighlightMaterial;
    public float highlightDuration = 0.2f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip removeSound;
    
    private Pin currentHoveredPin;
    private bool isPoking = false;
    
    void OnEnable()
    {
        if (pokeInteractor != null)
        {
            pokeInteractor.selectEntered.AddListener(OnPokeEnter);
            pokeInteractor.selectExited.AddListener(OnPokeExit);
        }
    }
    
    void OnDisable()
    {
        if (pokeInteractor != null)
        {
            pokeInteractor.selectEntered.RemoveListener(OnPokeEnter);
            pokeInteractor.selectExited.RemoveListener(OnPokeExit);
        }
    }
    
    void OnPokeEnter(SelectEnterEventArgs args)
    {
        // Check if poked object is a pin
        Pin pin = args.interactableObject.transform.GetComponent<Pin>();
        
        if (pin != null && pin.IsInChain())
        {
            isPoking = true;
            currentHoveredPin = pin;
            
            // Visual feedback
            StartCoroutine(FlashPin(pin));
            
            // Optional hover sound
            if (audioSource != null && removeSound != null)
                audioSource.PlayOneShot(removeSound);
                
            // Remove the pin
            threadManager.RemovePin(pin);
        }
    }
    
    void OnPokeExit(SelectExitEventArgs args)
    {
        isPoking = false;
        currentHoveredPin = null;
    }
    
    System.Collections.IEnumerator FlashPin(Pin pin)
    {
        Renderer renderer = pin.GetComponent<Renderer>();
        if (renderer != null && removeHighlightMaterial != null)
        {
            Material originalMaterial = renderer.material;
            renderer.material = removeHighlightMaterial;
            yield return new WaitForSeconds(highlightDuration);
            renderer.material = originalMaterial;
        }
    }
}