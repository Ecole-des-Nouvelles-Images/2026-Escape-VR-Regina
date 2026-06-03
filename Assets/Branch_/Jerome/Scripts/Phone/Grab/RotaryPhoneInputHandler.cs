using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotaryPhoneInputHandler : Puzzle
{
    [Tooltip("7 digit numbers")]
    [SerializeField] private List<string> _validNumbers = new() {"1234567", "7654321"};
    
    [Header("Vibration Integration")]
    [Tooltip("Reference to the rotary handle that should vibrate")]
    [SerializeField] private Transform _phoneHandle;
    
    [Tooltip("How long incoming calls should vibrate")]
    [SerializeField] private float _incomingCallVibrationDuration = 3f;
    
    [Tooltip("Whether to use the wrong number vibration pattern")]
    [SerializeField] private bool _useWrongNumberPattern = true;
    
    private readonly Stack<char> _numberStack = new();
    private readonly List<char> _phoneNumber = new();
    private RotaryHandleVibrator _vibrator;
    private bool _hasIncomingCall = false;
    
    private void Start()
    {
        // Validate and clean valid numbers
        foreach (string number in _validNumbers.Where(number => number.Length != 7).ToList()) 
            _validNumbers.Remove(number);
        
        // Disable input until puzzle starts
        GetComponent<Collider>().enabled = false;
        
        // Setup vibrator component
        SetupVibrator();
    }
    
    private void SetupVibrator()
    {
        if (_phoneHandle == null)
        {
            Debug.LogWarning("Phone handle reference not assigned! Vibration will not work.");
            return;
        }
        
        _vibrator = _phoneHandle.GetComponent<RotaryHandleVibrator>();
        if (_vibrator == null)
            _vibrator = _phoneHandle.gameObject.AddComponent<RotaryHandleVibrator>();
    }
    
    /// <summary>
    /// Validates the current phone number sequence
    /// </summary>
    private void ValidateCurrentSequence()
    {
        // No digits entered yet - nothing to validate
        if (_phoneNumber.Count == 0) return;

        string currentSequence = new(_phoneNumber.ToArray());
        
        // Check if this exact sequence matches any valid number
        if (_validNumbers.Contains(currentSequence))
        {
            // Complete match! We've entered a full valid number
            Solve();
            _phoneNumber.Clear();
            return;
        }
        
        // Check if this sequence is a prefix of any valid number
        bool isValidPrefix = _validNumbers.Any(validNumber => validNumber.StartsWith(currentSequence));
        
        if (!isValidPrefix)
        {
            // Invalid sequence - wrong number!
            WrongNumber();
            _phoneNumber.Clear();
            _numberStack.Clear();
            return;
        }
        
        // Valid prefix, continue accepting digits
    }
    
    private void WrongNumber()
    {
        Debug.Log("Wrong number entered! Resetting...");
        
        // Use the dedicated wrong number vibration method
        if (_vibrator != null)
        {
            if (_useWrongNumberPattern)
                _vibrator.WrongNumber();
            else
                _vibrator.StartVibration(0.3f);
        }
        
        // Clear any incoming call flag if active
        if (_hasIncomingCall)
        {
            CancelInvoke(nameof(StopIncomingCall));
            StopIncomingCall();
        }
        
        // Add other error feedback (sound, visual effect, etc.)
    }
    
    public override void Solve()
    {
        // Stop any incoming call vibration
        if (_hasIncomingCall)
            StopIncomingCall();
        
        // Call base Solve method
        base.Solve();
        
        Debug.Log("Phone puzzle solved! Correct number entered.");
    }

    private void StartIncomingCall()
    {
        if (_hasIncomingCall) return;
        
        _hasIncomingCall = true;
        
        if (_vibrator != null)
        {
            _vibrator.StartVibration(_incomingCallVibrationDuration);
            Debug.Log("Phone is ringing! Handle vibrating...");
        }
        
        Invoke(nameof(StopIncomingCall), _incomingCallVibrationDuration);
    }
    
    private void StopIncomingCall()
    {
        _hasIncomingCall = false;
        
        if (_vibrator != null && _vibrator.IsVibrating())
            _vibrator.StopVibration();
    }
    
    private void OnEnable()
    {
        EventBus.OnPuzzleSolved += OnPuzzleSolved;
    }
    
    private void OnPuzzleSolved(Puzzle obj)
    {
        if (obj.PuzzleID != 1) return;
        
        GetComponent<Collider>().enabled = true;
        StartIncomingCall();
    }
    
    private void OnDisable()
    {
        EventBus.OnPuzzleSolved -= OnPuzzleSolved;
        
        if (_vibrator != null && _vibrator.IsVibrating())
            _vibrator.StopVibration();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Number")) 
            _numberStack.Push(Convert.ToChar(other.gameObject.name));
    }
    
    public void ReleaseDial()
    {
        if (_numberStack.Count == 0) return;
        
        // Add the digit to our current sequence
        _phoneNumber.Add(_numberStack.Pop());
        
        // Validate the new sequence
        ValidateCurrentSequence();
        
        // Clear the stack for the next digit
        _numberStack.Clear();
    }
}