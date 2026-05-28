using UnityEngine;
using System.Collections;

public class FlashingRenderer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Renderer to flash (will auto-find if empty)")]
    [SerializeField] private Renderer _targetRenderer;
    
    [Header("Flash Settings")]
    [SerializeField] private Material _flashMaterial;
    [SerializeField] private float _flashDuration = 0.5f;      // time for one complete flash cycle
    [SerializeField] private AnimationCurve _flashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Material _originalMaterial;
    private bool _isFlashing = false;
    private Coroutine _flashCoroutine;
    private float _currentFlashAlpha = 0f;

    void Awake()
    {
        if (_targetRenderer == null)
            _targetRenderer = GetComponent<Renderer>();
        
        if (_targetRenderer == null)
        {
            Debug.LogError($"No Renderer found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Store original material
        _originalMaterial = _targetRenderer.material;
        
        // Create instance of flash material so we can modify it safely
        if (_flashMaterial != null)
            _flashMaterial = new Material(_flashMaterial);
        else
            Debug.LogWarning($"No flash material assigned on {gameObject.name}");
    }

    [ContextMenu("Flash")]
    public void StartFlashing()
    {
        if (_isFlashing) return;
        if (_targetRenderer == null || _flashMaterial == null) return;
        
        _isFlashing = true;
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    [ContextMenu("StopFlashing")]
    public void StopFlashing()
    {
        _isFlashing = false;
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }
        
        // Restore original material
        if (_targetRenderer != null && _originalMaterial != null)
            _targetRenderer.material = _originalMaterial;
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;
        
        while (_isFlashing)
        {
            timer += Time.deltaTime;
            float t = (timer % _flashDuration) / _flashDuration;
            
            // Get alpha value between 0 and 1 based on curve
            float alpha = _flashCurve.Evaluate(Mathf.PingPong(t, 1f));
            _currentFlashAlpha = alpha;
            
            // Apply flash material with current alpha
            if (_flashMaterial != null)
            {
                // Set alpha on flash material
                Color color = _flashMaterial.color;
                color.a = alpha;
                _flashMaterial.color = color;
                
                _targetRenderer.material = _flashMaterial;
                
            }
            
            yield return null;
        }
    }
    
    // Optional: Public method to change flash material at runtime
    public void SetFlashMaterial(Material newFlashMaterial)
    {
        _flashMaterial = newFlashMaterial;
    }
}