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
        foreach (string number in _validNumbers.Where(number => number.Length != 7)) _validNumbers.Remove(number);
        
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
        if (other.CompareTag("Number")) _numberStack.Push(Convert.ToChar(other.gameObject.name));
    }

    // No-OP for now, but it would compare the number to any we have listed as correct otherwise empty the number
    private void CompareNumber(List<char> phoneNumber)
    {
        string number = new(phoneNumber.ToArray());
        Debug.Log(number);
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
        }
        _phoneNumber.Clear();
    }

    public void ReleaseDial()
    {
        if (_numberStack.Count == 0)  return;
        
        // take from the top of the stack
        _phoneNumber.Add(_numberStack.Pop());
        if (_phoneNumber.Count == 7) CompareNumber(_phoneNumber);

        _numberStack.Clear();
    }
    public void InputNumber()
    {
        _phoneNumber.Add(_numberStack.Pop());
        _numberStack.Clear();

        if (_phoneNumber.Count >= 7)
        {
            CompareNumber(_phoneNumber);
        }
    }

}
