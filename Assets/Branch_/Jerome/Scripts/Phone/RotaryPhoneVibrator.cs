using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

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
    [SerializeField] private Vector3 _vibrationAxis = new(0, 0, 1);
    
    [Header("Wrong Number Settings")]
    [Tooltip("Maximum rotation angle in degrees during wrong number feedback")]
    [SerializeField] private float _vibrationAngleWrong = 4f;
    
    [Tooltip("Speed/frequency of the wrong number vibration (higher = faster shaking)")]
    [SerializeField] private float _vibrationFrequencyWrong = 90f;
    
    [Tooltip("Controls the intensity pattern for wrong number feedback")]
    [SerializeField] private AnimationCurve _vibrationCurveWrong = AnimationCurve.EaseInOut(0, 1, 1, 1);
    
    [Tooltip("Which local rotation axis to vibrate on for wrong number (usually Y for shaking)")]
    [SerializeField] private Vector3 _vibrationAxisWrong = new(0, 1, 0);
    
    [Tooltip("Duration of the wrong number vibration in seconds")]
    [SerializeField] private float _wrongNumberDuration = 0.3f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip _audioRing;
    [SerializeField] private AudioClip _audioWrongNumber;
    private AudioSource _audioSource;
    
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
    
    // Current vibration parameters (can be overridden by special effects)
    private float _currentAngle;
    private float _currentFrequency;
    private AnimationCurve _currentCurve;
    private Vector3 _currentAxis;
    private float _currentVibrationDuration = -1f; // -1 means infinite
    
    private void Awake()
    {
        // Store the original rotation
        _originalRotation = transform.localRotation;
        
        // Generate a random seed for this instance
        _randomSeed = Random.Range(0f, 100f);
        
        _audioSource = GetComponent<AudioSource>();
        
        // Set default vibration parameters
        SetDefaultVibrationParameters();
    }
    
    private void SetDefaultVibrationParameters()
    {
        _currentAngle = _vibrationAngle;
        _currentFrequency = _vibrationFrequency;
        _currentCurve = _vibrationCurve;
        _currentAxis = _vibrationAxis;
    }
    
    private void SetWrongNumberParameters()
    {
        _currentAngle = _vibrationAngleWrong;
        _currentFrequency = _vibrationFrequencyWrong;
        _currentCurve = _vibrationCurveWrong;
        _currentAxis = _vibrationAxisWrong;
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
        // Reset to default parameters
        SetDefaultVibrationParameters();
        
        // Stop any existing vibration
        if (_vibrationCoroutine != null)
            StopCoroutine(_vibrationCoroutine);
        
        _isVibrating = true;
        _vibrationTime = 0f;
        _currentVibrationDuration = duration;
     
        _audioSource.Stop();
        _audioSource.clip = _audioRing;
        _audioSource.Play();
        
        if (duration > 0)
            _vibrationCoroutine = StartCoroutine(StopVibrationAfterDelay(duration));
    }
    
    /// <summary>
    /// Quick animation to show the wrong number was input
    /// Uses the wrong number vibration settings for a distinct feedback feel
    /// </summary>
    [ContextMenu("Wrong Number")]
    public void WrongNumber()
    {
        // Stop any current vibration
        if (_vibrationCoroutine != null)
        {
            StopCoroutine(_vibrationCoroutine);
            _vibrationCoroutine = null;
        }
        
        // Set wrong number parameters
        SetWrongNumberParameters();
        
        // Start vibration with wrong number duration
        _isVibrating = true;
        _vibrationTime = 0f;
        _currentVibrationDuration = _wrongNumberDuration;
        
        _audioSource.Stop();
        _audioSource.clip = _audioWrongNumber;
        _audioSource.Play();
        
        // Auto-stop after wrong number duration
        _vibrationCoroutine = StartCoroutine(StopVibrationAfterDelay(_wrongNumberDuration));
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
        _currentVibrationDuration = -1f;
        
        // Smoothly return to original rotation
        if (gameObject.activeInHierarchy)
            StartCoroutine(ReturnToOriginalRotation());
        else
            transform.localRotation = _originalRotation;
        
        // Reset to default parameters for next vibration
        SetDefaultVibrationParameters();
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
        _currentAngle = Mathf.Clamp(intensity, 0f, 10f);
    }
    
    /// <summary>
    /// Change vibration frequency on the fly
    /// </summary>
    public void SetVibrationFrequency(float frequency)
    {
        _currentFrequency = Mathf.Clamp(frequency, 1f, 100f);
    }
    
    /// <summary>
    /// Gets normalized vibration time (0-1) for curve evaluation.
    /// For infinite vibrations, returns a looping pattern.
    /// For finite vibrations, returns progress percentage.
    /// </summary>
    private float GetNormalizedVibrationTime()
    {
        // Calculate real elapsed time (not affected by frequency multiplier)
        float realTime = _vibrationTime / _currentFrequency;
        
        if (_currentVibrationDuration > 0)
        {
            // Finite vibration: evaluate curve based on progress (0 at start, 1 at end)
            float progress = Mathf.Clamp01(realTime / _currentVibrationDuration);
            return progress;
        }
        else
        {
            // Infinite vibration: loop through curve every 2 seconds
            float loopDuration = 2f;
            float normalizedLoop = (realTime % loopDuration) / loopDuration;
            return normalizedLoop;
        }
    }
    
    private void Update()
    {
        if (!_isVibrating) return;
        
        // Increment vibration time
        _vibrationTime += Time.deltaTime * _currentFrequency;
        
        // Calculate base vibration value (sin wave for oscillation)
        float vibrationValue = Mathf.Sin(_vibrationTime);
        
        // Get intensity multiplier from curve based on normalized time
        float normalizedTime = GetNormalizedVibrationTime();
        float intensityMultiplier = _currentCurve.Evaluate(normalizedTime);
        
        // Calculate final angle
        float angleOffset = vibrationValue * _currentAngle * intensityMultiplier;
        
        // Add randomness if enabled
        if (_addRandomness)
        {
            float randomOffset = Mathf.PerlinNoise(_randomSeed, _vibrationTime * 2f) - 0.5f;
            angleOffset += randomOffset * _randomnessAmount * _currentAngle;
        }
        
        // Create rotation based on current vibration axis
        Quaternion vibrationRotation = Quaternion.Euler(
            _currentAxis.x * angleOffset,
            _currentAxis.y * angleOffset,
            _currentAxis.z * angleOffset
        );
        
        // Apply the rotation
        transform.localRotation = _originalRotation * vibrationRotation;
    }
}