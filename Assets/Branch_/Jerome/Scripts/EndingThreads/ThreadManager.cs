using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    [Header("Thread Visual")]
    [SerializeField] private LineRenderer _threadLine;
    [SerializeField] private Material _threadMaterial;
    [SerializeField] private float _threadWidth = 0.02f;
    
    [Header("Win Condition")]
    [SerializeField] private List<string> _targetSequence; // Set in Inspector: ["A","C","B","D","F"]
    
    // Runtime data
    private readonly List<Pin> _currentOrder = new();
    private bool _gameWon;

    private void Start()
    {
        if (_threadLine == null)
            _threadLine = GetComponent<LineRenderer>();
            
        if (_threadLine != null)
        {
            _threadLine.startWidth = _threadWidth;
            _threadLine.endWidth = _threadWidth;
            _threadLine.material = _threadMaterial;
            _threadLine.positionCount = 0;
        }
    }
    
    public bool AddPin(Pin newPin)
    {
        if (_gameWon) return false;
        
        // Prevent adding same pin twice
        if (_currentOrder.Contains(newPin))
        {
            Debug.Log($"Pin {newPin.PinID} already in chain");
            return false;
        }
        
        // Add to chain
        _currentOrder.Add(newPin);
        newPin.SetInChain(true);
        newPin.PlayAttachEffect();
        
        // Update visual thread
        UpdateThreadVisual();
        
        // Log current sequence
        string sequence = _currentOrder.Count > 0 ? 
            string.Join(" → ", _currentOrder.Select(p => p.PinID)) : "Empty";
        Debug.Log($"Current sequence: {sequence}");
        
        // Check win condition
        CheckWinCondition();
        
        return true;
    }
    
    public bool RemovePin(Pin targetPin)
    {
        if (_gameWon) return false;
        
        int index = _currentOrder.IndexOf(targetPin);
        if (index == -1)
        {
            Debug.Log($"Pin {targetPin.PinID} not in chain");
            return false;
        }
        
        // Remove from chain
        _currentOrder.RemoveAt(index);
        targetPin.SetInChain(false);
        
        // Update visual thread (automatically reconnects neighbors)
        UpdateThreadVisual();
        
        // Log new sequence
        string sequence = _currentOrder.Count > 0 ? 
            string.Join(" → ", _currentOrder.Select(p => p.PinID)) : "Empty";
        Debug.Log($"After removal: {sequence}");
        
        return true;
    }

    private void UpdateThreadVisual()
    {
        if (_threadLine == null) return;
        
        // Need at least 2 points to draw a line
        if (_currentOrder.Count < 2)
        {
            _threadLine.positionCount = 0;
            return;
        }
        
        // Build positions list - linear path only between consecutive pins
        List<Vector3> positions = new();
        
        // Add all pins' connection points in order
        foreach (Pin pin in _currentOrder)
        {
            positions.Add(pin.ConnectionPoint != null ? pin.ConnectionPoint.position : pin.transform.position);
        }
        
        // Update line renderer
        _threadLine.positionCount = positions.Count;
        _threadLine.SetPositions(positions.ToArray());
    }

    private void CheckWinCondition()
    {
        // Convert current pins to IDs
        List<string> currentIDs = _currentOrder.Select(p => p.PinID).ToList();
        
        // Check if sequences match exactly
        bool sequenceMatches = currentIDs.SequenceEqual(_targetSequence);
        
        if (sequenceMatches && currentIDs.Count == _targetSequence.Count)
        {
            Win();
        }
    }

    private void Win()
    {
        _gameWon = true;
        Debug.Log("🎉 VICTORY! Correct sequence achieved! 🎉");
            
        // Change thread color to gold
        if (_threadLine != null)
        {
            _threadLine.startColor = Color.yellow;
            _threadLine.endColor = Color.yellow;
        }
        
        // You can add more win effects here:
        // - Particle effects
        // - UI panel
        // - Load next scene
    }
    
    // Debug/Editor method to reset the game
    public void ResetGame()
    {
        foreach (Pin pin in _currentOrder)
        {
            pin.SetInChain(false);
        }
        _currentOrder.Clear();
        UpdateThreadVisual();
        _gameWon = false;
        
        if (_threadLine != null && _threadMaterial != null)
        {
            _threadLine.startColor = Color.white;
            _threadLine.endColor = Color.white;
            _threadLine.material = _threadMaterial;
        }
            
        Debug.Log("Game reset");
    }
    
    // Optional: Get current sequence for UI display
    public List<string> GetCurrentSequence()
    {
        return _currentOrder.Select(p => p.PinID).ToList();
    }
}