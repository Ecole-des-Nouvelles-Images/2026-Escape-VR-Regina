using UnityEngine;
using System.Collections;

public class RotaryHandleVibrator : MonoBehaviour
{
    [Header("Vibration Settings")]
    [Tooltip("Maximum rotation angle in degrees during vibration")]
    [SerializeField] private float _vibrationAngle = 2f;
    
    [Tooltip("Speed/frequency of the vibration (higher = faster shaking)")]
    [SerializeField] private float _vibrationFrequency = 30f;
    
    [Tooltip("Controls the intensity pattern over time (X-axis: 0-1 normalized time, Y-axis: 0-1 intensity multiplier)")]
    [SerializeField] private AnimationCurve _vibrationCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    
    [Tooltip("Which local rotation axis to vibrate on (usually Z for rotary handles)")]
    [SerializeField] private Vector3 _vibrationAxis = new Vector3(0, 0, 1);
    
    [Header("Optional Features")]
    [Tooltip("Randomize the vibration pattern slightly for more realism")]
    [SerializeField] private bool _addRandomness = true;
    
    [Tooltip("Amount of randomness to add (0-1)")]
    [SerializeField] private float _randomnessAmount = 0.3f;
    
    private Quaternion _originalRotation;
    private bool _isVibrating = false;
    private float _vibrationTime = 0f;
    private float _randomSeed;
    private Coroutine _vibrationCoroutine;
    
    private void Awake()
    {
        // Store the original rotation
        _originalRotation = transform.localRotation;
        
        // Generate a random seed for this instance
        _randomSeed = Random.Range(0f, 100f);
    }
    
    private void OnEnable()
    {
        // Ensure we reset to original rotation when enabled
        if (!_isVibrating)
            transform.localRotation = _originalRotation;
    }
    
    private void OnDisable()
    {
        // Stop any ongoing vibration when disabled
        if (_isVibrating)
            StopVibration();
    }
    
    /// <summary>
    /// Start vibrating the handle indefinitely
    /// </summary>
    [ContextMenu("Start Vibration")]
    public void StartVibration()
    {
        StartVibration(-1f);
    }
    
    /// <summary>
    /// Start vibrating the handle for a specific duration
    /// </summary>
    /// <param name="duration">Duration in seconds (-1 for infinite)</param>
    public void StartVibration(float duration)
    {
        // Stop any existing vibration
        if (_vibrationCoroutine != null)
            StopCoroutine(_vibrationCoroutine);
        
        _isVibrating = true;
        _vibrationTime = 0f;
        
        if (duration > 0)
            _vibrationCoroutine = StartCoroutine(StopVibrationAfterDelay(duration));
    }
    
    /// <summary>
    /// Stop the handle vibration immediately
    /// </summary>
    [ContextMenu("Stop Vibration")]
    public void StopVibration()
    {
        if (_vibrationCoroutine != null)
        {
            StopCoroutine(_vibrationCoroutine);
            _vibrationCoroutine = null;
        }
        
        _isVibrating = false;
        
        // Smoothly return to original rotation
        if (gameObject.activeInHierarchy)
            StartCoroutine(ReturnToOriginalRotation());
        else
            transform.localRotation = _originalRotation;
    }
    
    /// <summary>
    /// Smoothly return the handle to its original rotation
    /// </summary>
    private IEnumerator ReturnToOriginalRotation()
    {
        float returnDuration = 0.2f;
        float elapsed = 0f;
        Quaternion startRotation = transform.localRotation;
        
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            transform.localRotation = Quaternion.Slerp(startRotation, _originalRotation, t);
            yield return null;
        }
        
        transform.localRotation = _originalRotation;
    }
    
    /// <summary>
    /// Stop vibration after specified delay
    /// </summary>
    private IEnumerator StopVibrationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopVibration();
    }
    
    /// <summary>
    /// Check if the handle is currently vibrating
    /// </summary>
    public bool IsVibrating()
    {
        return _isVibrating;
    }
    
    /// <summary>
    /// Change vibration intensity on the fly
    /// </summary>
    public void SetVibrationIntensity(float intensity)
    {
        _vibrationAngle = Mathf.Clamp(intensity, 0f, 10f);
    }
    
    /// <summary>
    /// Change vibration frequency on the fly
    /// </summary>
    public void SetVibrationFrequency(float frequency)
    {
        _vibrationFrequency = Mathf.Clamp(frequency, 1f, 100f);
    }
    
    private void Update()
    {
        if (!_isVibrating) return;
        
        // Increment vibration time
        _vibrationTime += Time.deltaTime * _vibrationFrequency;
        
        // Calculate base vibration value
        float vibrationValue = Mathf.Sin(_vibrationTime);
        
        // Apply animation curve modulation
        float curveTime = Mathf.PingPong(_vibrationTime * 0.5f, 1f);
        float intensityMultiplier = _vibrationCurve.Evaluate(curveTime);
        
        // Calculate final angle
        float angleOffset = vibrationValue * _vibrationAngle * intensityMultiplier;
        
        // Add randomness if enabled
        if (_addRandomness)
        {
            float randomOffset = Mathf.PerlinNoise(_randomSeed, _vibrationTime * 2f) - 0.5f;
            angleOffset += randomOffset * _randomnessAmount * _vibrationAngle;
        }
        
        // Create rotation based on vibration axis
        Quaternion vibrationRotation = Quaternion.Euler(
            _vibrationAxis.x * angleOffset,
            _vibrationAxis.y * angleOffset,
            _vibrationAxis.z * angleOffset
        );
        
        // Apply the rotation
        transform.localRotation = _originalRotation * vibrationRotation;
    }
}