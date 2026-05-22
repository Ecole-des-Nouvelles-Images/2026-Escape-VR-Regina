using UnityEngine;
using System.Collections;

public class FlashingRenderer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Renderer to flash (will auto-find if empty)")]
    [SerializeField] private Renderer targetRenderer;
    
    [Header("Flash Settings")]
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.5f;      // time for one complete flash cycle
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Material originalMaterial;
    private bool isFlashing = false;
    private Coroutine flashCoroutine;
    private float currentFlashAlpha = 0f;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
        
        if (targetRenderer == null)
        {
            Debug.LogError($"No Renderer found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Store original material
        originalMaterial = targetRenderer.material;
        
        // Create instance of flash material so we can modify it safely
        if (flashMaterial != null)
            flashMaterial = new Material(flashMaterial);
        else
            Debug.LogWarning($"No flash material assigned on {gameObject.name}");
    }

    [ContextMenu("Flash")]
    public void StartFlashing()
    {
        if (isFlashing) return;
        if (targetRenderer == null || flashMaterial == null) return;
        
        isFlashing = true;
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    [ContextMenu("StopFlashing")]
    public void StopFlashing()
    {
        isFlashing = false;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        // Restore original material
        if (targetRenderer != null && originalMaterial != null)
            targetRenderer.material = originalMaterial;
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;
        
        while (isFlashing)
        {
            timer += Time.deltaTime;
            float t = (timer % flashDuration) / flashDuration;
            
            // Get alpha value between 0 and 1 based on curve
            float alpha = flashCurve.Evaluate(Mathf.PingPong(t, 1f));
            currentFlashAlpha = alpha;
            
            // Apply flash material with current alpha
            if (flashMaterial != null)
            {
                // Set alpha on flash material
                Color color = flashMaterial.color;
                color.a = alpha;
                flashMaterial.color = color;
                
                targetRenderer.material = flashMaterial;
                
            }
            
            yield return null;
        }
    }
    
    // Optional: Public method to change flash material at runtime
    public void SetFlashMaterial(Material newFlashMaterial)
    {
        flashMaterial = newFlashMaterial;
    }
}