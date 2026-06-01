using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotaryPhoneInputHandler : Puzzle
{
    [Tooltip("7 digit numbers")]
    [SerializeField] private List<string> _validNumbers = new() {"1234567", "7654321"};
    
    private readonly Stack<char> _numberStack = new();
    private readonly List<char> _phoneNumber = new();

    private void Start()
    {
        foreach (string number in _validNumbers.Where(number => number.Length != 7).ToList()) 
            _validNumbers.Remove(number);
        
        // TODO : During Polish if we want to add some more easter eggs we could still allow the phone to be used
        // TODO : but just disable the wining number.
        GetComponent<Collider>().enabled = false; // Won't accept inputs until the puzzle has started
    }

    private void OnEnable()
    {
        EventBus.OnPuzzleSolved += OnPuzzleSolved;
    }

    private void OnPuzzleSolved(Puzzle obj)
    {
        if (obj.PuzzleID != 1) return;
        
        GetComponent<Collider>().enabled = true;
    }
    
    private void OnDisable()
    {
        EventBus.OnPuzzleSolved -= OnPuzzleSolved;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Number")) 
            _numberStack.Push(Convert.ToChar(other.gameObject.name));
    }

    private void CompareNumber(List<char> phoneNumber)
    {
        string number = new(phoneNumber.ToArray());
        int index = _validNumbers.IndexOf(number);
        
        // Define what number corresponds to what action/event
        switch (index)
        {
            case 0:
                Solve();
                break;
            case 1:
                break;
            case 2:
                break;
            default:
                WrongNumber();
                break;
        }
        _phoneNumber.Clear();
    }

    private void CheckCurrentSequence()
    {
        if (_phoneNumber.Count == 0) return;
        
        string currentSequence = new(_phoneNumber.ToArray());
        
        // Check if the current sequence matches the start of any valid number
        bool isValidPrefix = _validNumbers.Any(validNumber => validNumber.StartsWith(currentSequence));
        
        // If it's not a valid prefix, and we've entered at least one digit, it's wrong
        if (!isValidPrefix && _phoneNumber.Count > 0)
        {
            WrongNumber();
            _phoneNumber.Clear();
            _numberStack.Clear();
        }
        // If we have a complete 7-digit number, check if it exactly matches any valid number
        else if (_phoneNumber.Count == 7)
        {
            string fullNumber = new(_phoneNumber.ToArray());
            if (_validNumbers.Contains(fullNumber))
            {
                Solve();
                _phoneNumber.Clear();
            }
            else
            {
                WrongNumber();
                _phoneNumber.Clear();
                _numberStack.Clear();
            }
        }
    }

    private void WrongNumber()
    {
        // TODO: Add error feedback here (sound, visual effect, etc.)
    }

    public void ReleaseDial()
    {
        if (_numberStack.Count == 0) return;
        
        // take from the top of the stack
        _phoneNumber.Add(_numberStack.Pop());
        
        // Check the current sequence after adding the digit
        CheckCurrentSequence();
        
        _numberStack.Clear();
    }
    
    public void ClearSequence()
    {
        _phoneNumber.Clear();
        _numberStack.Clear();
    }
}