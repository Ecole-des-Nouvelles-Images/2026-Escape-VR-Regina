using System;
using UnityEngine;

public class RotaryDial : MonoBehaviour
{
    [Header("Dial Settings")]
    public float MaxRotationAngle = 330f;  // How far dial can rotate
    public float ReturnSpeed = 5f;          // Speed dial returns to start
    
    private float _currentRotation = 0f;
    private bool _isPushing = false;
    private Transform _fingerTransform;
    private Transform _pivotParent;           // Reference to parent transform
    private Quaternion _startLocalRotation;
    private Vector3 _startLocalPosition;
    
    void Start()
    {
        _pivotParent = transform.parent;
        
        // Store initial local transform relative to parent
        _startLocalRotation = transform.localRotation;
        _startLocalPosition = transform.localPosition;
    }
    
    void Update()
    {
        if (_isPushing && _fingerTransform != null && _pivotParent != null)
        {
            // Get finger position in parent's local space
            Vector3 fingerPosLocal = _pivotParent.InverseTransformPoint(_fingerTransform.position);
            Vector3 dialPosLocal = transform.localPosition;
            
            // Vector from parent center to finger (in local space)
            Vector3 toFinger = fingerPosLocal - Vector3.zero; // Parent center is (0,0,0) locally
            toFinger.y = 0; // Ignore vertical movement for rotation
            
            // Calculate angle from forward direction to finger
            float targetAngle = Vector3.SignedAngle(Vector3.forward, toFinger, Vector3.up);
            
            // Constrain angle to dial range (assuming max rotation is clockwise)
            targetAngle = Mathf.Clamp(targetAngle, 0f, MaxRotationAngle);
            
            // Smooth rotation to finger position
            _currentRotation = Mathf.Lerp(_currentRotation, targetAngle, Time.deltaTime * 15f);
            
            // Apply rotation around parent's Y axis
            ApplyRotation(_currentRotation);
            
        }
        else if (_currentRotation > 0f)
        {
            // Return to start when not pushing
            _currentRotation = Mathf.Lerp(_currentRotation, 0f, Time.deltaTime * ReturnSpeed);
            
            if (_currentRotation < 0.5f)
            {
                _currentRotation = 0f;
                ResetTransform();
            }
            else
            {
                ApplyRotation(_currentRotation);
            }
        }
    }
    
    void ApplyRotation(float angle)
    {
        if (_pivotParent == null) return;
        
        // Reset to initial local transform first
        transform.localRotation = _startLocalRotation;
        transform.localPosition = _startLocalPosition;
        
        // Apply rotation around parent's Y axis
        transform.RotateAround(_pivotParent.position, Vector3.up, -angle);
    }
    
    void ResetTransform()
    {
        transform.localRotation = _startLocalRotation;
        transform.localPosition = _startLocalPosition;
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand") || other.CompareTag("Finger"))
        {
            _isPushing = true;
            _fingerTransform = other.transform;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") || other.CompareTag("Finger"))
        {
            _isPushing = false;
            _fingerTransform = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Number")
        {
            Debug.Log(other.name);
        }
    }
}