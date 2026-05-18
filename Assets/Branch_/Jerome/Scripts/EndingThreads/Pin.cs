using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pin : MonoBehaviour
{
    [Header("Identification")]
    public string PinID; // "A", "B", "C", etc.
    
    [Header("Attachment Point")]
    public Transform ConnectionPoint; // Where thread visually attaches
    
    [Header("Visual Feedback")]
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Material _pokedMaterial; // Material when poked
    [SerializeField] private ParticleSystem _attachParticles;
    
    [Header("Poke Settings")]
    [SerializeField] private float _pokeCooldown = 0.5f; // Prevent rapid removal
    
    private Renderer _pinRenderer;
    private bool _isInChain = false;
    private bool _isPokeOnCooldown = false;
    private Material _originalMaterial;

    private void Awake()
    {
        _pinRenderer = GetComponent<Renderer>();
        if (_pinRenderer != null && _defaultMaterial != null)
            _originalMaterial = _defaultMaterial;
    }
    
    public void SetInChain(bool inChain)
    {
        _isInChain = inChain;
        // Optional: change color or effect when part of chain
        if (_isInChain)
        {
            // Slight highlight to show it's in the chain
            if (_pinRenderer != null && _highlightMaterial != null)
                _pinRenderer.material = _highlightMaterial;
        }
        else
        {
            if (_pinRenderer != null && _defaultMaterial != null)
                _pinRenderer.material = _defaultMaterial;
        }
    }
    
    public bool IsInChain()
    {
        return _isInChain;
    }
    
    public void PlayAttachEffect()
    {
        if (_attachParticles != null)
            _attachParticles.Play();
    }
    
    // Called by XRPokeInteractor when pin is poked
    public void OnPoked()
    {
        if (!_isInChain) return;
        if (_isPokeOnCooldown) return;
        
        // Visual feedback for being poked
        if (_pinRenderer != null && _pokedMaterial != null)
        {
            _pinRenderer.material = _pokedMaterial;
            Invoke(nameof(ResetMaterialAfterPoke), 0.1f);
        }
        
        // Trigger cooldown
        _isPokeOnCooldown = true;
        Invoke(nameof(ResetPokeCooldown), _pokeCooldown);
        
        // Find ThreadManager and request removal
        ThreadManager threadManager = FindObjectOfType<ThreadManager>();
        if (threadManager != null)
        {
            threadManager.RemovePin(this);
        }
    }

    private void ResetMaterialAfterPoke()
    {
        if (_pinRenderer != null)
        {
            if (_isInChain && _highlightMaterial != null)
                _pinRenderer.material = _highlightMaterial;
            else if (!_isInChain && _defaultMaterial != null)
                _pinRenderer.material = _defaultMaterial;
        }
    }

    private void ResetPokeCooldown()
    {
        _isPokeOnCooldown = false;
    }
}