using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotaryPhoneButton : MonoBehaviour
{
    [SerializeField] private Rigidbody _rbDial;
    [SerializeField] private List<float> _numberAngles; // Manually assign angles for numbers 0-9
    [SerializeField] private RotaryPhoneInputHandler2 _inputHandler;
    
    [SerializeField] private float _returnDelay = 0.5f;
    [SerializeField] private float _rotateSpeed = 360f; // Degrees per second
    [SerializeField] private float _returnSpeed = 180f; // Degrees per second
    
    private Queue<int> _numbers = new();
    private bool _isProcessing = false;
    private Quaternion _homeRotation;
    
    private void Start()
    {
        if (!_rbDial) 
            _rbDial = GetComponent<Rigidbody>();
        
        // Store the initial rotation as the home position
        _homeRotation = _rbDial.transform.localRotation;
        
        // Ensure the Rigidbody is kinematic for precise rotation control
        _rbDial.isKinematic = true;
        
        // Validate number angles
        if (_numberAngles.Count != 10)
        {
            Debug.LogWarning($"Number angles count is {_numberAngles.Count}, but should be 10 (0-9)");
        }
    }
    
    private void Update()
    {
        // Continuously try to process the queue
        if (!_isProcessing && _numbers.Count > 0)
        {
            StartCoroutine(ProcessRotationQueue());
        }
    }
    
    private IEnumerator ProcessRotationQueue()
    {
        _isProcessing = true;
        
        while (_numbers.Count > 0)
        {
            int number = _numbers.Dequeue();
            yield return StartCoroutine(RotateToNumber(number));
            yield return new WaitForSeconds(_returnDelay);
            yield return StartCoroutine(ReturnToHome());
        }
        
        _isProcessing = false;
    }
    
    private IEnumerator RotateToNumber(int number)
    {
        float targetAngle = _numberAngles[number];
        float startAngle = _rbDial.transform.localEulerAngles.z;
        Quaternion testAngle = _rbDial.transform.rotation;
        float testAngle2 = _rbDial.transform.rotation.eulerAngles.z;
        Debug.Log($"Rotate from {startAngle} or {testAngle} or {testAngle2}");
        
        // Calculate clockwise rotation needed
        float angleDelta;
        if (targetAngle >= startAngle)
        {
            angleDelta = targetAngle - startAngle;
        }
        else
        {
            // Wrap around 360 degrees
            angleDelta = (360 - startAngle) + targetAngle;
        }
        
        Debug.Log($"Rotating to number {number}: from {startAngle}° to {targetAngle}° (delta: {angleDelta}°)");
        
        float rotatedThisFrame = 0f;
        
        while (rotatedThisFrame < angleDelta)
        {
            float step = _rotateSpeed * Time.deltaTime;
            float remaining = angleDelta - rotatedThisFrame;
            float rotateAmount = Mathf.Min(step, remaining);
            
            // Rotate around local Z axis
            _rbDial.transform.Rotate(0, 0, rotateAmount, Space.Self);
            rotatedThisFrame += rotateAmount;
            
            yield return null;
        }
        
        // Ensure we hit the exact angle
        Vector3 exactRotation = _rbDial.transform.localEulerAngles;
        exactRotation.z = targetAngle;
        _rbDial.transform.localRotation = Quaternion.Euler(exactRotation);
        
        // Notify the input handler that a number has been dialed
        OnNumberDialed(number);
    }
    
    private IEnumerator ReturnToHome()
    {
        float startAngle = _rbDial.transform.localEulerAngles.z;
        float homeAngle = _homeRotation.eulerAngles.z;
        
        // Calculate clockwise rotation back home
        float angleDelta;
        if (homeAngle >= startAngle)
        {
            angleDelta = homeAngle - startAngle;
        }
        else
        {
            angleDelta = (360 - startAngle) + homeAngle;
        }
        
        Debug.Log($"Returning home: from {startAngle}° to {homeAngle}° (delta: {angleDelta}°)");
        
        float rotatedThisFrame = 0f;
        
        while (rotatedThisFrame < angleDelta)
        {
            float step = _returnSpeed * Time.deltaTime;
            float remaining = angleDelta - rotatedThisFrame;
            float rotateAmount = Mathf.Min(step, remaining);
            
            // Rotate around local Z axis back to home
            _rbDial.transform.Rotate(0, 0, -rotateAmount, Space.Self);
            rotatedThisFrame += rotateAmount;
            
            yield return null;
        }
        
        // Ensure we end exactly at home rotation
        _rbDial.transform.localRotation = _homeRotation;
    }
    
    private void OnNumberDialed(int number)
    {
        if (_inputHandler)
        {
            _inputHandler.OnNumberDialed(number);
        }
        else
        {
            Debug.LogWarning($"No input handler assigned! Dialed number: {number}");
        }
    }
    
    public void OnClick(int number)
    {
        // Validate number range
        if (number < 0 || number >= _numberAngles.Count)
        {
            Debug.LogError($"Number {number} is out of range (0-{_numberAngles.Count - 1})");
            return;
        }
        
        _numbers.Enqueue(number);
        Debug.Log($"Number {number} added to queue. Queue size: {_numbers.Count}");
    }
    
    // Optional: Reset method for testing or restarting
    public void ResetDial()
    {
        StopAllCoroutines();
        _numbers.Clear();
        _isProcessing = false;
        _rbDial.transform.localRotation = _homeRotation;
        Debug.Log("Dial reset to home position");
    }
    
    // Optional: Check if dial is currently rotating
    public bool IsRotating()
    {
        return _isProcessing;
    }
}