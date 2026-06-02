using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RotaryDialGrab : XRBaseInteractable
{
    [Header("Rotation Settings")]
    [SerializeField] private float _maxRotationAngle = 330f;
    [SerializeField] private float _returnSpeed = 300f;
    [SerializeField] private float _returnDelay = 0.2f;
    
    [Header("Sensitivity & Smoothing")]
    [SerializeField] private float _rotationSensitivity = 2.5f; // Multiplier for rotation sensitivity
    [SerializeField] private float _smoothingFactor = 0.15f; // Lower = smoother but more lag
    [SerializeField] private float _angularVelocityLimit = 720f; // Max degrees per second
    
    [Header("Haptics")]
    [SerializeField] private float _hapticOnStart = 0.1f;
    [SerializeField] private float _hapticOnStop = 0.15f;
    [SerializeField] private float _hapticOnMaxRotation = 0.2f;
    
    [Header("References")]
    [SerializeField] private Transform _dialToRotate;
    [SerializeField] private RotaryPhoneInputHandler _inputHandler;
    
    // Rotation tracking
    private Quaternion _initialRotation;
    private float _currentRotationDelta;
    private bool _isDialing;
    private bool _isReturning;
    private float _returnTimer;
    
    // Enhanced 1:1 tracking variables
    private IXRSelectInteractor _currentInteractor;
    private Vector3 _lastHandPosition;
    private Vector3 _lastHandDirection;
    private float _lastAngleDelta;
    private float _smoothRotationDelta;
    private float _angularVelocity;
    private bool _hasStartedRotation;
    
    // For better sensitivity calculation
    private float _dialRadius = 0.1f; // Approximate radius of the dial in meters
    private Vector3 _dialCenter;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (!_inputHandler)
            _inputHandler = FindFirstObjectByType<RotaryPhoneInputHandler>();
    }
    
    private void Start()
    {
        if (_dialToRotate != null)
        {
            _initialRotation = _dialToRotate.rotation;
            _dialCenter = GetDialCenter();
            
            // Estimate dial radius from collider or mesh bounds
            if (TryGetComponent<Collider>(out var col))
            {
                _dialRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
            }
            else if (_dialToRotate.TryGetComponent<MeshRenderer>(out var renderer))
            {
                _dialRadius = Mathf.Max(renderer.bounds.extents.x, renderer.bounds.extents.y);
            }
        }
        else
        {
            _dialToRotate = transform;
            _initialRotation = transform.rotation;
            Debug.LogWarning("Dial to rotate reference is missing! Using this transform.", this);
        }
    }
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        
        _returnTimer = 0;
        _isReturning = false;
        _isDialing = true;
        _currentInteractor = args.interactorObject;
        _hasStartedRotation = false;
        _angularVelocity = 0f;
        _smoothRotationDelta = _currentRotationDelta;
        
        // Get initial hand position
        Transform attachTransform = args.interactorObject.GetAttachTransform(this);
        if (attachTransform != null)
        {
            _lastHandPosition = attachTransform.position;
            _lastHandDirection = ProjectOntoRotationPlane((_lastHandPosition - _dialCenter).normalized);
            
            // Provide haptic feedback on grab
            SendHapticFeedback(_hapticOnStart);
        }
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        if (_isDialing)
        {
            // Calculate momentum for natural release feel
            float momentum = Mathf.Clamp(Mathf.Abs(_angularVelocity) / _angularVelocityLimit, 0f, 0.5f);
            _currentRotationDelta = _smoothRotationDelta;
            StopDialing(momentum);
            
            SendHapticFeedback(_hapticOnStop);
        }
        
        _currentInteractor = null;
        _hasStartedRotation = false;
        _angularVelocity = 0f;
    }
    
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (_isDialing && _currentInteractor != null)
            {
                UpdateRotationEnhanced();
            }
            
            UpdateRotationMechanics();
        }
    }
    
    private void UpdateRotationEnhanced()
    {
        Transform attachTransform = _currentInteractor.GetAttachTransform(this);
        if (attachTransform == null) return;
        
        Vector3 currentHandPosition = attachTransform.position;
        Vector3 currentHandDirection = (currentHandPosition - _dialCenter).normalized;
        currentHandDirection = ProjectOntoRotationPlane(currentHandDirection);
        
        if (!_hasStartedRotation)
        {
            // Initialize tracking
            _lastHandDirection = currentHandDirection;
            _lastAngleDelta = 0f;
            _hasStartedRotation = true;
            return;
        }
        
        // Calculate angle using cross product for better sensitivity
        Vector3 cross = Vector3.Cross(_lastHandDirection, currentHandDirection);
        float signedAngle = Vector3.SignedAngle(_lastHandDirection, currentHandDirection, Vector3.forward);
        
        // Alternative more sensitive calculation using arc length
        float angularDistance = Vector3.Angle(_lastHandDirection, currentHandDirection);
        float handMovementDistance = Vector3.Distance(_lastHandPosition, currentHandPosition);
        
        // Enhanced sensitivity: use both angular change and hand arc movement
        float adjustedAngle = signedAngle * _rotationSensitivity;
        
        // If hand moves significantly, boost sensitivity
        if (handMovementDistance > 0.02f && angularDistance < 30f)
        {
            float arcBoost = Mathf.Lerp(1f, 2f, handMovementDistance / 0.1f);
            adjustedAngle *= arcBoost;
        }
        
        // Calculate angular velocity for smoothing
        _angularVelocity = adjustedAngle / Time.deltaTime;
        _angularVelocity = Mathf.Clamp(_angularVelocity, -_angularVelocityLimit, _angularVelocityLimit);
        
        // Apply smoothing to reduce jitter
        float targetDelta = _currentRotationDelta + adjustedAngle;
        
        // Apply constraints (clockwise only)
        if (targetDelta < 0)
        {
            targetDelta = 0;
            SendHapticFeedback(_hapticOnStop * 0.5f);
        }
        
        if (targetDelta > _maxRotationAngle)
        {
            targetDelta = _maxRotationAngle;
            SendHapticFeedback(_hapticOnMaxRotation);
        }
        
        // Smooth the rotation delta
        _smoothRotationDelta = Mathf.Lerp(_smoothRotationDelta, targetDelta, 1f - _smoothingFactor);
        _currentRotationDelta = _smoothRotationDelta;
        
        // Update tracking for next frame
        _lastHandPosition = currentHandPosition;
        _lastHandDirection = currentHandDirection;
        _lastAngleDelta = adjustedAngle;
    }
    
    private void UpdateRotationMechanics()
    {
        if (_isDialing)
        {
            ApplyRotation();
        }
        else if (_isReturning)
        {
            // Add easing for smoother return
            float returnAmount = _returnSpeed * Time.deltaTime;
            if (_currentRotationDelta < returnAmount)
            {
                _currentRotationDelta = 0;
                _isReturning = false;
            }
            else
            {
                // Ease out for natural feel
                float t = _currentRotationDelta / _maxRotationAngle;
                float easedReturn = returnAmount * (1f + t * 0.5f);
                _currentRotationDelta -= easedReturn;
            }
            
            ApplyRotation();
        }
        else if (_returnTimer > 0)
        {
            _returnTimer -= Time.deltaTime;
            if (_returnTimer <= 0)
            {
                _isReturning = true;
                _returnTimer = 0;
            }
        }
    }
    
    private void StopDialing(float momentum = 0f)
    {
        if (!_isDialing) return;
        
        _isDialing = false;
        _returnTimer = _returnDelay;
        
        // Optionally add momentum to the return
        if (momentum > 0.05f && _currentRotationDelta < _maxRotationAngle * 0.8f)
        {
            // Add slight momentum before returning
            _currentRotationDelta += momentum * 30f;
            _currentRotationDelta = Mathf.Clamp(_currentRotationDelta, 0, _maxRotationAngle);
        }
        
        if (_inputHandler)
            _inputHandler.ReleaseDial();
    }
    
    private void ApplyRotation()
    {
        if (_dialToRotate == null) return;
        
        // Use local rotation for better compatibility with parent transforms
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, _currentRotationDelta);
        _dialToRotate.rotation = _initialRotation * deltaRotation;
    }
    
    private Vector3 GetDialCenter()
    {
        // Better pivot detection - use the center of the dial's visual
        Transform pivotTransform = _dialToRotate.parent != null ? _dialToRotate.parent : _dialToRotate;
        
        // Get the actual center of the dial mesh or collider
        if (_dialToRotate.TryGetComponent<Collider>(out var col))
            return col.bounds.center;
        
        return pivotTransform.position;
    }
    
    private Vector3 ProjectOntoRotationPlane(Vector3 direction)
    {
        // Use the dial's up vector for proper orientation
        Vector3 axis = _dialToRotate.parent != null ? _dialToRotate.parent.up : Vector3.forward;
        
        float axisComponent = Vector3.Dot(direction, axis);
        Vector3 projected = direction - (axis * axisComponent);
        
        if (projected.magnitude < 0.001f)
        {
            // Find perpendicular vector
            Vector3 perp = Vector3.Cross(axis, Vector3.right);
            if (perp.magnitude < 0.001f)
                perp = Vector3.Cross(axis, Vector3.up);
            return perp.normalized;
        }
        
        return projected.normalized;
    }
    
    private void SendHapticFeedback(float intensity)
    {
        if (_currentInteractor is XRBaseInputInteractor controllerInteractor && intensity > 0)
        {
            controllerInteractor.SendHapticImpulse(intensity, 0.05f);
        }
    }
    
    public bool IsDialing() => _isDialing;
    public bool IsReturning() => _isReturning;
    public float GetCurrentRotation() => _currentRotationDelta;
    
    public void ResetDial()
    {
        _currentRotationDelta = 0;
        _smoothRotationDelta = 0;
        _isDialing = false;
        _isReturning = false;
        _returnTimer = 0;
        _hasStartedRotation = false;
        _angularVelocity = 0f;
        ApplyRotation();
    }
    
    public void SetRotationDelta(float deltaDegrees)
    {
        _currentRotationDelta = Mathf.Clamp(deltaDegrees, 0f, _maxRotationAngle);
        _smoothRotationDelta = _currentRotationDelta;
        ApplyRotation();
    }
}