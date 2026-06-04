using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotaryPhoneButton : MonoBehaviour
{
    [SerializeField] private Rigidbody _rbDial;
    [Tooltip("11 numbers expected 0-9 + home")]
    [SerializeField] private List<float> _numberAngles; // Index 0-9 for numbers, index 10 for home rotation
    [SerializeField] private RotaryPhoneInputHandler2 _inputHandler;

    [SerializeField] private float _returnDelay = 0.5f;
    [SerializeField] private float _rotateSpeed = 360f; // Degrees per second (clockwise)
    [SerializeField] private float _returnSpeed = 180f; // Degrees per second (anti-clockwise)
    
    private readonly Queue<int> _numbers = new();
    private bool _isProcessing = false;
    private float _homeAngle;
    
    private void Start()
    {
        if (!_rbDial) 
            _rbDial = GetComponent<Rigidbody>();
        
        // Home angle is at index 10 (the 11th element)
        if (_numberAngles.Count > 10)
        {
            _homeAngle = _numberAngles[10];
        }
        else
        {
            Debug.LogError("NumberAngles list must have 11 elements (0-9 for numbers, 10 for home)");
            _homeAngle = _rbDial.transform.localEulerAngles.z;
        }
        
        // Ensure the Rigidbody is kinematic
        _rbDial.isKinematic = true;
    }
    
    private void Update()
    {
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
            yield return new WaitForSeconds(_returnDelay);
        }
        
        _isProcessing = false;
    }
    
    private IEnumerator RotateToNumber(int number)
    {
        float targetAngle = _numberAngles[number];
        float startAngle = _rbDial.transform.localEulerAngles.z;
        
        float angleDelta;
        if (targetAngle >= startAngle)
        {
            angleDelta = targetAngle - startAngle;
        }
        else
        {
            angleDelta = (360 - startAngle) + targetAngle;
        }
        
        float rotatedThisFrame = 0f;
        
        while (rotatedThisFrame < angleDelta)
        {
            float step = _rotateSpeed * Time.deltaTime;
            float remaining = angleDelta - rotatedThisFrame;
            float rotateAmount = Mathf.Min(step, remaining);
            
            _rbDial.transform.Rotate(0, 0, rotateAmount, Space.Self);
            rotatedThisFrame += rotateAmount;
            
            yield return null;
        }
        
        OnNumberDialed(number);
    }
    
    private IEnumerator ReturnToHome()
    {
        float currentAngle = _rbDial.transform.localEulerAngles.z;
        float targetAngle = _homeAngle;
        
        // Calculate anti-clockwise delta (going backwards)
        float angleDelta;
        if (currentAngle >= targetAngle)
        {
            angleDelta = currentAngle - targetAngle;
        }
        else
        {
            angleDelta = currentAngle + (360 - targetAngle);
        }
        
        float rotatedThisFrame = 0f;
        
        while (rotatedThisFrame < angleDelta)
        {
            float step = _returnSpeed * Time.deltaTime;
            float remaining = angleDelta - rotatedThisFrame;
            float rotateAmount = Mathf.Min(step, remaining);
            
            // Rotate anti-clockwise (negative)
            _rbDial.transform.Rotate(0, 0, -rotateAmount, Space.Self);
            rotatedThisFrame += rotateAmount;
            
            yield return null;
        }
        
        // Snap to exact home angle
        Vector3 exactRotation = _rbDial.transform.localEulerAngles;
        exactRotation.z = _homeAngle;
        _rbDial.transform.localRotation = Quaternion.Euler(exactRotation);
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
        if (number < 0 || number > 9)
        {
            Debug.LogError($"Number {number} is out of range (0-9)");
            return;
        }
        
        _numbers.Enqueue(number);
        Debug.Log($"Number {number} added to queue. Queue size: {_numbers.Count}");
    }
    
    public void ResetDial()
    {
        StopAllCoroutines();
        _numbers.Clear();
        _isProcessing = false;
        
        Vector3 homeRotation = _rbDial.transform.localEulerAngles;
        homeRotation.z = _homeAngle;
        _rbDial.transform.localRotation = Quaternion.Euler(homeRotation);
    }
}