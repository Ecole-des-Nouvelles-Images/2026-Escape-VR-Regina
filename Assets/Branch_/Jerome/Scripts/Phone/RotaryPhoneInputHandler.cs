using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RotaryPhoneInputHandler : Puzzle
{
    private readonly Stack<char> _numberStack = new();
    private readonly List<char> _phoneNumber = new();
    private readonly List<string> _validNumbers = new() {"1234567", "7654321"};
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Number")) _numberStack.Push(Convert.ToChar(other.gameObject.name));
    }

    private void Start()
    {
        _numberStack.Push('0'); // Edge Case : Player releases dial immediately
    }

    // No-OP for now, but it would compare the number to any we have listed as correct otherwise empty the number
    private void CompareNumber(List<char> phoneNumber)
    {
        string number = new(phoneNumber.ToArray());
        
        int index = _validNumbers.IndexOf(number);
        if (index == -1) return;
        
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
    }

    public void ReleaseDial()
    {
        // take from the top of the stack
        _phoneNumber.Add(_numberStack.Pop());
        if (_phoneNumber.Count == 7) CompareNumber(_phoneNumber);

        _numberStack.Clear();
        _numberStack.Push('0'); // Edge Case : Player releases dial immediately
        
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
