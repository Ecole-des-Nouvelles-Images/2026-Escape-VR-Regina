using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RotaryDialGrab : XRGrabInteractable
{
    [Header("Rotary Settings")]
    [SerializeField] private float _maxRotationAngle = 45f;
    [SerializeField] private float _springReturnSpeed = 8f;
    [SerializeField] private float _grabSensitivity = 5f;
    [SerializeField] private RotaryPhoneInputHandler _rotaryPhoneInput;
    
    private float _currentRotation = 0f;
    private float _targetRotation = 0f;
    private IXRSelectInteractor _currentInteractor;
    private Vector3 _lastInteractorPosition;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        // Disable movement entirely
        trackPosition = false;
        trackRotation = false;
        
        // Prevent object from being moved
        movementType = MovementType.VelocityTracking;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _currentInteractor = args.interactorObject;
        
        // Store initial position for delta calculation
        Transform attachTransform = args.interactorObject.GetAttachTransform(this);
        if (attachTransform != null)
        {
            _lastInteractorPosition = attachTransform.position;
        }
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        _currentInteractor = null;
        
        // Optional: Spring return to 0 when released
        _targetRotation = 0f;
    }
    
    private void Update()
    {
        if (_currentInteractor != null)
        {
            // Safely get the attach transform
            Transform attachTransform = _currentInteractor.GetAttachTransform(this);
            if (attachTransform == null) return;
            
            // Calculate delta movement around the dial's center
            Vector3 dialCenter = transform.parent != null ? transform.parent.position : transform.position;
            Vector3 interactorPos = attachTransform.position;
            
            // Get direction vectors from dial center
            Vector3 currentDirection = (interactorPos - dialCenter).normalized;
            Vector3 lastDirection = (_lastInteractorPosition - dialCenter).normalized;
            
            // Calculate angle delta
            float angleDelta = Vector3.SignedAngle(lastDirection, currentDirection, transform.forward);
            
            // ONLY allow clockwise rotation (positive delta)
            // But allow counter-clockwise movement if it doesn't go below 0
            float newTarget = _targetRotation + (angleDelta * _grabSensitivity);
            
            // Clamp between 0 and max rotation (clockwise only)
            newTarget = Mathf.Clamp(newTarget, 0f, _maxRotationAngle);
            
            // Only update if the new target is valid
            // This prevents counter-clockwise rotation past 0
            if (newTarget >= 0f && newTarget <= _maxRotationAngle)
            {
                _targetRotation = newTarget;
            }
            
            // Smooth follow
            _currentRotation = Mathf.Lerp(_currentRotation, _targetRotation, Time.deltaTime * 15f);
            
            // Update last position
            _lastInteractorPosition = interactorPos;
        }
        else
        {
            // Spring return to zero
            _targetRotation = Mathf.Lerp(_targetRotation, 0f, Time.deltaTime * _springReturnSpeed);
            _currentRotation = Mathf.Lerp(_currentRotation, _targetRotation, Time.deltaTime * 15f);
        }
        
        // Apply rotation - only on Y axis (assuming that's your dial rotation axis)
        transform.localRotation = Quaternion.Euler(0f, _currentRotation, 0f);
    }
}