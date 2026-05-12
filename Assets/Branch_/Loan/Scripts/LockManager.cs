using System;
using System.Collections.Generic;
using UnityEngine;

public class LockManager : Puzzle
{
    [SerializeField] private List<int> _values;
    [SerializeField] private List<int> _codes;

    private void Start()
    {
        while (_values.Count < _codes.Count)
        {
            _values.Add(0); 
        }
        
        while (_values.Count > _codes.Count)
        {
            _values.RemoveAt(_values.Count - 1);
        }
    }

    public void GetValue(int value, int index)
    {
        if (index >= 0 && index < _values.Count)
        {
            _values[index] = value;
            CodeCheck();
        }
    }

    private void CodeCheck()
    {
        if (PuzzleSequenceManager.Instance.CurrentPuzzle != this)
            return;
        
        if (_values.Count <= 2)
        {
            if (_values[0] == _codes[0] && _values[1] == _codes[1] )
            {
               Solve();
            }
            return;
        }
        
        if (_values[0] == _codes[0] && _values[1] == _codes[1] && _values[2] == _codes[2])
        {
            Solve();
        }
    }

    public override void Solve()
    {
        base.Solve();
        
        // toute fonctionnalité une fois le Puzzle fini (exemple : jouer un sons, changement de scene, ect...)
        
        
    }
    
    
}
