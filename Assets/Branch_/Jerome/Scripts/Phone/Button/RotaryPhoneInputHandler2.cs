using System.Collections.Generic;
using UnityEngine;

public class RotaryPhoneInputHandler2 : Puzzle
{
    [Tooltip("7 digit numbers")]
    [SerializeField] private List<string> _validNumbers = new() {"1234567", "7654321"};
    [SerializeField] private RotaryPhoneButton _rotaryPhoneButton;
    
    private readonly List<int> _phoneNumber = new();
    private bool _isAwaitingInput = true;
    
    private void Start()
    {
        // Validate the rotary phone button reference
        if (!_rotaryPhoneButton)
        {
            _rotaryPhoneButton = FindObjectOfType<RotaryPhoneButton>();
            if (!_rotaryPhoneButton)
            {
                Debug.LogError("RotaryPhoneButton not found in scene!");
            }
        }
        
        // Validate phone numbers have correct length
        for (int i = _validNumbers.Count - 1; i >= 0; i--)
        {
            if (_validNumbers[i].Length != 7)
            {
                Debug.LogWarning($"Removed invalid number {_validNumbers[i]} - must be 7 digits");
                _validNumbers.RemoveAt(i);
            }
        }
    }
    
    // This will be called by the RotaryPhoneButton when a number is fully dialed
    public void OnNumberDialed(int number)
    {
        if (!_isAwaitingInput)
        {
            Debug.Log("Phone is currently processing, please wait...");
            return;
        }
        
        // Add the dialed number to our sequence
        _phoneNumber.Add(number);
        Debug.Log($"Dialed: {number}. Current sequence: {string.Join("", _phoneNumber)}");
        
        // Check if we've reached 7 digits
        if (_phoneNumber.Count == 7)
        {
            CompareNumber();
        }
    }
    
    private void CompareNumber()
    {
        _isAwaitingInput = false;
        
        // Convert the list of integers to a string
        string dialedNumber = string.Join("", _phoneNumber);
        Debug.Log($"Complete number dialed: {dialedNumber}");
        
        // Check if the dialed number is valid
        int index = _validNumbers.IndexOf(dialedNumber);
        
        if (index != -1)
        {
            // Valid number found - trigger the corresponding action
            Debug.Log($"Valid number detected at index {index}: {_validNumbers[index]}");
            
            switch (index)
            {
                case 0:
                    Debug.Log("Number 0 matched - Solving puzzle!");
                    Solve();
                    break;
                case 1:
                    Debug.Log("Number 1 matched - Triggering alternative action");
                    // Add custom action for second number here
                    break;
                case 2:
                    Debug.Log("Number 2 matched - Triggering alternative action");
                    // Add custom action for third number here
                    break;
                default:
                    Debug.Log($"Number matched but no action defined for index {index}");
                    break;
            }
        }
        else
        {
            // Invalid number - provide feedback and reset
            Debug.Log($"Invalid number {dialedNumber} - Resetting input");
            OnInvalidNumber();
        }
        
        // Reset for next input
        ResetInput();
    }
    
    private void OnInvalidNumber()
    {
        // Optional: Add feedback for invalid number (sound, visual effect, etc.)
        // For example: PlayErrorSound();
        // Or: ShakePhoneVisual();
        
        // You could also call a method to make the dial shake or provide visual feedback
    }
    
    private void ResetInput()
    {
        _phoneNumber.Clear();
        _isAwaitingInput = true;
        Debug.Log("Input reset - Ready for new number");
    }
    
    // Optional: Manual reset method for external calls (e.g., reset button)
    public void ManualReset()
    {
        ResetInput();
        if (_rotaryPhoneButton)
        {
            _rotaryPhoneButton.ResetDial();
        }
    }
    
    // Optional: Method to get current input progress
    public string GetCurrentInputProgress()
    {
        return string.Join("", _phoneNumber);
    }
    
    // Optional: Method to add new valid numbers at runtime
    public void AddValidNumber(string number, int actionIndex = -1)
    {
        if (number.Length == 7 && !_validNumbers.Contains(number))
        {
            if (actionIndex >= 0 && actionIndex < _validNumbers.Count)
            {
                _validNumbers.Insert(actionIndex, number);
            }
            else
            {
                _validNumbers.Add(number);
            }
            Debug.Log($"Added new valid number: {number}");
        }
        else
        {
            Debug.LogWarning($"Failed to add {number} - must be 7 digits and not a duplicate");
        }
    }
}