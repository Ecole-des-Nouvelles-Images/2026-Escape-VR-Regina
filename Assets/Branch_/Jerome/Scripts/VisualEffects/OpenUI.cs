using UnityEngine;

public class OpenUI : MonoBehaviour
{
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private bool isScaling = false;
    private float startTime;
    private Vector3 originalScale;
    private Vector3 targetScale;

    // Public method to start scaling from current scale to Y = 1
    [ContextMenu("Scale up")]
    public void ScaleToFullHeight()
    {
        StartScaling(1f);
    }

    // Public method to start scaling from current scale to Y = 0
    [ContextMenu("Scale down")]
    public void ScaleToZeroHeight()
    {
        StartScaling(0f);
    }

    // Public method to scale to a specific Y value (0 to 1)
    public void ScaleToY(float targetY)
    {
        targetY = Mathf.Clamp01(targetY);
        StartScaling(targetY);
    }

    private void StartScaling(float targetY)
    {
        originalScale = transform.localScale;
        targetScale = new Vector3(originalScale.x, targetY, originalScale.z);
        startTime = Time.time;
        isScaling = true;
    }

    private void Update()
    {
        if (!isScaling) return;
        
        float elapsed = Time.time - startTime;
        float t = Mathf.Clamp01(elapsed / animationDuration);
            
        // Evaluate the animation curve (value between 0 and 1)
        float curveValue = scaleCurve.Evaluate(t);
            
        // Interpolate based on the curve
        float currentY = Mathf.Lerp(originalScale.y, targetScale.y, curveValue);
        transform.localScale = new Vector3(originalScale.x, currentY, originalScale.z);

        if (!(t >= 1f)) return;
        
        // Animation complete - ensure exact target value
        transform.localScale = targetScale;
        isScaling = false;
    }
}