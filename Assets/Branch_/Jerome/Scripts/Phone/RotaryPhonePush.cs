using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Collider))]
public class RotaryDialMechanism : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float _maxRotation = 330f;      // degrees from rest to stop
    [SerializeField] private float _returnSpeed = 300f;      // degrees/sec during spring return
    [SerializeField] private AnimationCurve _returnCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    [Header("References")]
    [SerializeField] private Transform _dialTransform;        // the rotating part
    [SerializeField] private Transform _dialCenter;           // pivot point (center of dial)
    [SerializeField] private LayerMask _fingerLayer;
    
    [Header("Rotation Axis")]
    [SerializeField] private Vector3 _rotationAxis = Vector3.forward; // Local axis to rotate around
    
    public LayerMask FingerLayer => _fingerLayer;
    
    [System.Serializable]
    public class HoleInfo
    {
        public Collider triggerCollider;
        public float initialLocalAngle;   // angle of this hole when dial rotation = 0 (degrees)
        public int digit;
    }
    [SerializeField] private List<HoleInfo> _holes;
    
    // State
    private Quaternion _initialLocalRotation;  // Store the dial's starting local rotation
    private float _currentRotationDelta = 0f;   // How much ADDITIONAL rotation has been applied (degrees)
    private bool _isReturning = false;
    private HoleInfo _activeHole = null;
    private Transform _activeFinger = null;
    private Coroutine _returnCoroutine = null;
    
    // Events
    public System.Action<int> OnDigitDialed;
    public System.Action<float> OnRotationChanged;
    
    private void Start()
    {
        if (_dialTransform == null) _dialTransform = transform;
        if (_dialCenter == null) _dialCenter = _dialTransform;
        
        // Store the initial local rotation of the dial
        _initialLocalRotation = _dialTransform.localRotation;
        
        // Register trigger events for each hole
        foreach (var hole in _holes)
        {
            if (hole.triggerCollider != null)
            {
                var triggerEvent = hole.triggerCollider.gameObject.AddComponent<HoleTriggerHandler>();
                triggerEvent.Init(this, hole);
            }
            else
            {
                Debug.LogError($"Hole {hole.digit} has no trigger collider!", this);
            }
        }
        
        ApplyRotation();
    }
    
    public void OnFingerEnterHole(HoleInfo hole, Transform finger)
    {
        if (_isReturning || _activeHole != null || finger == null) return;
        
        _activeHole = hole;
        _activeFinger = finger;
        _isReturning = false;
        
        if (_returnCoroutine != null)
            StopCoroutine(_returnCoroutine);
    }
    
    public void OnFingerStayHole(HoleInfo hole, Transform finger)
    {
        if (_activeHole != hole || _isReturning) return;
        
        // Project finger position onto dial plane
        Vector3 toFinger = finger.position - _dialCenter.position;
        Vector3 dialForward = _dialTransform.forward;
        Vector3 projected = Vector3.ProjectOnPlane(toFinger, dialForward);
        
        if (projected.sqrMagnitude < 0.001f) return;
        
        // Get local axes of the dial
        Vector3 dialRight = _dialTransform.right;
        Vector3 dialUp = _dialTransform.up;
        
        // Compute angle of finger relative to dial's local axes
        float fingerAngle = Mathf.Atan2(Vector3.Dot(projected, dialUp), 
                                        Vector3.Dot(projected, dialRight)) * Mathf.Rad2Deg;
        
        // Calculate target delta rotation (finger angle - hole's rest angle)
        float targetDelta = fingerAngle - hole.initialLocalAngle;
        
        // Normalize angle to range [-180, 180]
        if (targetDelta < -180f) targetDelta += 360f;
        if (targetDelta > 180f) targetDelta -= 360f;
        
        // Clamp to allowed range [0, _maxRotation]
        targetDelta = Mathf.Clamp(targetDelta, 0f, _maxRotation);
        
        // Update the current rotation delta
        _currentRotationDelta = targetDelta;
        ApplyRotation();
    }
    
    public void OnFingerExitHole(HoleInfo hole, Transform finger)
    {
        if (_activeHole != hole) return;
        
        // Determine which digit was dialed based on how far we rotated
        int dialedDigit = GetDigitForRotation(_currentRotationDelta);
        if (dialedDigit >= 0)
            OnDigitDialed?.Invoke(dialedDigit);
        
        // Start return sequence
        _activeHole = null;
        _activeFinger = null;
        _returnCoroutine = StartCoroutine(ReturnToRest());
    }
    
    private IEnumerator ReturnToRest()
    {
        _isReturning = true;
        float startDelta = _currentRotationDelta;
        float elapsed = 0f;
        float duration = startDelta / _returnSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = _returnCurve.Evaluate(elapsed / duration);
            _currentRotationDelta = Mathf.Lerp(startDelta, 0f, t);
            ApplyRotation();
            yield return null;
        }
        
        _currentRotationDelta = 0f;
        ApplyRotation();
        _isReturning = false;
        _returnCoroutine = null;
    }
    
    private void ApplyRotation()
    {
        if (_dialTransform == null) return;
        
        // Create a local rotation delta based on the accumulated angle
        // Multiply the initial rotation by the delta rotation (additive)
        Quaternion deltaRotation = Quaternion.AngleAxis(_currentRotationDelta, _rotationAxis);
        
        // Apply additive rotation on top of the initial local rotation
        _dialTransform.localRotation = _initialLocalRotation * deltaRotation;
        
        OnRotationChanged?.Invoke(_currentRotationDelta);
    }
    
    private int GetDigitForRotation(float rotation)
    {
        if (rotation < 5f) return -1; // Threshold to ignore tiny rotations
        
        // Map rotation angle to digit (0-9)
        // Typically: full rotation (330°) = digit 0, 33° = digit 1, 66° = digit 2, etc.
        float step = _maxRotation / 10f;
        int digit = Mathf.FloorToInt(rotation / step);
        
        // Adjust: 0 is the last digit (full rotation)
        if (digit >= 10) digit = 0;
        
        return digit;
    }
    
    // Public API
    public void ResetDial()
    {
        if (_returnCoroutine != null)
            StopCoroutine(_returnCoroutine);
        
        _activeHole = null;
        _activeFinger = null;
        _isReturning = false;
        _currentRotationDelta = 0f;
        ApplyRotation();
    }
    
    public bool IsDialing() => _activeHole != null;
    public bool IsReturning() => _isReturning;
    public float GetCurrentRotation() => _currentRotationDelta;
}

// Helper component remains the same
public class HoleTriggerHandler : MonoBehaviour
{
    private RotaryDialMechanism _mechanism;
    private RotaryDialMechanism.HoleInfo _hole;
    private Transform _currentFinger = null;
    
    public void Init(RotaryDialMechanism mechanism, RotaryDialMechanism.HoleInfo hole)
    {
        _mechanism = mechanism;
        _hole = hole;
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_currentFinger != null) return;
        if (!IsFinger(other)) return;
        
        _currentFinger = other.transform;
        _mechanism.OnFingerEnterHole(_hole, _currentFinger);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (_currentFinger == null || other.transform != _currentFinger) return;
        _mechanism.OnFingerStayHole(_hole, _currentFinger);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (_currentFinger == null || other.transform != _currentFinger) return;
        _mechanism.OnFingerExitHole(_hole, _currentFinger);
        _currentFinger = null;
    }
    
    private bool IsFinger(Collider other)
    {
        return other.GetComponent<XRDirectInteractor>() != null ||
               (1 << other.gameObject.layer & _mechanism.FingerLayer) != 0;
    }
}