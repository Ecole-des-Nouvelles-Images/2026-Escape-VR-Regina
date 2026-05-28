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
    
    [Header("References")]
    [SerializeField] private Transform _dialToRotate;
    [SerializeField] private RotaryPhoneInputHandler _inputHandler;
    
    // Rotation tracking (similar to push script)
    private Quaternion _initialRotation;
    private float _currentRotationDelta;
    private bool _isDialing;
    private bool _isReturning;
    private float _returnTimer;
    
    // 1:1 tracking variables
    private IXRSelectInteractor _currentInteractor;
    private float _rotationStartDelta;
    private Vector3 _rotationStartDirection;
    private bool _hasStartedRotation;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Find input handler if not assigned
        if (!_inputHandler)
            _inputHandler = FindFirstObjectByType<RotaryPhoneInputHandler>();
    }
    
    private void Start()
    {
        // Store the initial rotation of the dial
        if (_dialToRotate != null)
        {
            _initialRotation = _dialToRotate.rotation;
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
        
        // Cancel any pending return
        _returnTimer = 0;
        _isReturning = false;
        _isDialing = true;
        _currentInteractor = args.interactorObject;
        _hasStartedRotation = false;
        
        // Get initial hand position for 1:1 tracking
        Transform attachTransform = args.interactorObject.GetAttachTransform(this);
        if (attachTransform != null)
        {
            Vector3 dialCenter = GetDialCenter();
            Vector3 handPosition = attachTransform.position;
            Vector3 handDirection = (handPosition - dialCenter).normalized;
            
            // Project onto rotation plane
            handDirection = ProjectOntoRotationPlane(handDirection);

            _rotationStartDirection = handDirection;
            _rotationStartDelta = _currentRotationDelta;
            _hasStartedRotation = true;
        }
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        if (_isDialing)
        {
            StopDialing();
        }
        
        _currentInteractor = null;
        _hasStartedRotation = false;
    }
    
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (_isDialing && _currentInteractor != null && _hasStartedRotation)
            {
                UpdateRotation1To1();
            }
            
            UpdateRotationMechanics();
        }
    }
    
    private void UpdateRotation1To1()
    {
        // Safely get the attach transform
        Transform attachTransform = _currentInteractor.GetAttachTransform(this);
        if (attachTransform == null) return;
        
        // Get dial center and current hand position
        Vector3 dialCenter = GetDialCenter();
        Vector3 handPosition = attachTransform.position;
        Vector3 currentHandDirection = (handPosition - dialCenter).normalized;
        
        // Project onto rotation plane
        currentHandDirection = ProjectOntoRotationPlane(currentHandDirection);
        
        // Calculate the signed angle from start direction to current direction
        float angleDelta = Vector3.SignedAngle(_rotationStartDirection, currentHandDirection, Vector3.forward);
        
        // Calculate new rotation delta based on start delta + delta movement
        float newDelta = _rotationStartDelta + angleDelta;
        
        // Apply constraints (clockwise only - from 0 to max, never below 0)
        if (newDelta < 0)
        {
            newDelta = 0;
            // Reset start position when hitting the stop (gives mechanical feel)
            _rotationStartDelta = 0;
            _rotationStartDirection = currentHandDirection;
        }
        
        // Clamp to max rotation (like physical stop)
        if (newDelta > _maxRotationAngle)
        {
            newDelta = _maxRotationAngle;
            // Optional: Add haptic feedback when hitting max
            if (_currentInteractor is XRBaseInputInteractor controllerInteractor && newDelta >= _maxRotationAngle)
            {
                controllerInteractor.SendHapticImpulse(0.2f, 0.05f);
            }
        }
        
        // Apply 1:1 rotation
        _currentRotationDelta = newDelta;
    }
    
    private void UpdateRotationMechanics()
    {
        if (_isDialing)
        {
            // Apply rotation while dialing (1:1 already set in UpdateRotation1To1)
            ApplyRotation();
        }
        else if (_isReturning)
        {
            // Decrease the rotation delta back to zero (using same speed as push script)
            _currentRotationDelta -= _returnSpeed * Time.deltaTime;
            
            // Clamp to zero
            if (_currentRotationDelta <= 0)
            {
                _currentRotationDelta = 0;
                _isReturning = false;
            }
            
            ApplyRotation();
        }
        else if (_returnTimer > 0)
        {
            // Count down the return delay
            _returnTimer -= Time.deltaTime;
            if (_returnTimer <= 0)
            {
                _isReturning = true;
                _returnTimer = 0;
            }
        }
    }
    
    private void StopDialing()
    {
        if (!_isDialing) return;
        
        _isDialing = false;
        _returnTimer = _returnDelay;
        
        // Notify the input handler that dialing has stopped (same logic as push script)
        if (_inputHandler != null)
            _inputHandler.ReleaseDial();
    }
    
    private void ApplyRotation()
    {
        if (_dialToRotate == null) return;
        
        // Create the delta rotation from the current accumulated angle
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, _currentRotationDelta);

        // Combine the initial rotation with the delta rotation
        _dialToRotate.rotation = _initialRotation * deltaRotation;
    }
    
    private Vector3 GetDialCenter()
    {
        // Use parent as pivot point if available, otherwise use dial position
        Transform pivotTransform = _dialToRotate.parent != null ? _dialToRotate.parent : _dialToRotate;
        return pivotTransform.position;
    }
    
    private Vector3 ProjectOntoRotationPlane(Vector3 direction)
    {
        Vector3 axis = Vector3.forward;
        
        // Remove component along rotation axis to project onto rotation plane
        float axisComponent = Vector3.Dot(direction, axis);
        Vector3 projected = direction - (axis * axisComponent);
        
        // Normalize and handle zero vector
        if (projected.magnitude < 0.001f)
        {
            // Find a perpendicular vector to the axis
            if (Mathf.Abs(Vector3.Dot(axis, Vector3.right)) < 0.9f)
                return Vector3.Cross(axis, Vector3.right).normalized;
            else
                return Vector3.Cross(axis, Vector3.up).normalized;
        }
        
        return projected.normalized;
    }
    
    // Public methods for external use (matching push script interface)
    public bool IsDialing() => _isDialing;
    public bool IsReturning() => _isReturning;
    public float GetCurrentRotation() => _currentRotationDelta;
    
    // Reset the dial to its initial position (same as push script)
    public void ResetDial()
    {
        _currentRotationDelta = 0;
        _isDialing = false;
        _isReturning = false;
        _returnTimer = 0;
        _hasStartedRotation = false;
        ApplyRotation();
    }
    
    // Manually set the rotation delta (for external control)
    public void SetRotationDelta(float deltaDegrees)
    {
        _currentRotationDelta = Mathf.Clamp(deltaDegrees, 0f, _maxRotationAngle);
        ApplyRotation();
    }
}