using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pin : MonoBehaviour
{
    [Header("Identification")]
    public string pinID; // "A", "B", "C", etc.
    
    [Header("Attachment Point")]
    public Transform connectionPoint; // Where thread visually attaches
    
    [Header("Visual Feedback")]
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material pokedMaterial; // Material when poked
    public ParticleSystem attachParticles;
    
    [Header("Poke Settings")]
    public float pokeCooldown = 0.5f; // Prevent rapid removal
    
    private Renderer pinRenderer;
    private bool isInChain = false;
    private bool isPokeOnCooldown = false;
    private Material originalMaterial;
    
    void Awake()
    {
        pinRenderer = GetComponent<Renderer>();
        if (pinRenderer != null && defaultMaterial != null)
            originalMaterial = defaultMaterial;
    }
    
    public void SetInChain(bool inChain)
    {
        isInChain = inChain;
        // Optional: change color or effect when part of chain
        if (isInChain)
        {
            // Slight highlight to show it's in the chain
            if (pinRenderer != null && highlightMaterial != null)
                pinRenderer.material = highlightMaterial;
        }
        else
        {
            if (pinRenderer != null && defaultMaterial != null)
                pinRenderer.material = defaultMaterial;
        }
    }
    
    public bool IsInChain()
    {
        return isInChain;
    }
    
    public void PlayAttachEffect()
    {
        if (attachParticles != null)
            attachParticles.Play();
    }
    
    // Called by XRPokeInteractor when pin is poked
    public void OnPoked()
    {
        if (!isInChain) return;
        if (isPokeOnCooldown) return;
        
        // Visual feedback for being poked
        if (pinRenderer != null && pokedMaterial != null)
        {
            pinRenderer.material = pokedMaterial;
            Invoke(nameof(ResetMaterialAfterPoke), 0.1f);
        }
        
        // Trigger cooldown
        isPokeOnCooldown = true;
        Invoke(nameof(ResetPokeCooldown), pokeCooldown);
        
        // Find ThreadManager and request removal
        ThreadManager threadManager = FindObjectOfType<ThreadManager>();
        if (threadManager != null)
        {
            threadManager.RemovePin(this);
        }
    }
    
    void ResetMaterialAfterPoke()
    {
        if (pinRenderer != null)
        {
            if (isInChain && highlightMaterial != null)
                pinRenderer.material = highlightMaterial;
            else if (!isInChain && defaultMaterial != null)
                pinRenderer.material = defaultMaterial;
        }
    }
    
    void ResetPokeCooldown()
    {
        isPokeOnCooldown = false;
    }
}