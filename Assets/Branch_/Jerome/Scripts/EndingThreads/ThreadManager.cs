using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    [Header("Thread Visual")]
    [SerializeField] [Tooltip("Material applied to the thread (will be instanced per segment)")]
    private Material _threadMaterial;
    
    [SerializeField] [Tooltip("Width of the thread in world units")]
    [Range(0.005f, 0.1f)]
    private float _threadWidth = 0.02f;
    
    [SerializeField] [Tooltip("How many texture repeats per world unit (higher = more frequent pattern)")]
    [Range(0.1f, 3f)]
    private float _tilingFrequency = 1f;
    
    [SerializeField] [Tooltip("Number of points per segment (higher = smoother curves, lower = better performance)")]
    [Range(5, 50)]
    private int _pointsPerSegment = 20;
    
    [SerializeField] [Tooltip("How much the thread droops between pins (0 = straight, 1 = very saggy)")]
    [Range(0f, 1f)]
    private float _sagFactor = 0.3f;
    
    [Header("Thread Color")]
    [SerializeField] [Tooltip("Normal thread color")]
    private Color _normalColor = Color.white;
    
    [SerializeField] [Tooltip("Thread color when win condition is met")]
    private Color _winColor = Color.yellow;
    
    [Header("Win Condition")]
    [SerializeField] [Tooltip("Exact sequence of Pin IDs required to win (e.g., ['A','C','B','D','F'])")]
    private List<string> _targetSequence;
    
    // Runtime data
    private readonly List<Pin> _currentOrder = new();
    private readonly List<ThreadSegment> _currentSegments = new();
    private bool _gameWon;
    
    // Internal class to track segment data
    private class ThreadSegment
    {
        public LineRenderer LineRenderer;
        public GameObject GameObject;
        public Pin StartPin;
        public Pin EndPin;
        public float CachedLength;
    }
    
    private void Start()
    {
        if (!_threadMaterial)
            Debug.LogWarning("ThreadMaterial not assigned in ThreadManager", this);
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
        
        // Update visual thread
        UpdateThreadVisual();
        
        // Log new sequence
        string sequence = _currentOrder.Count > 0 ? 
            string.Join(" → ", _currentOrder.Select(p => p.PinID)) : "Empty";
        Debug.Log($"After removal: {sequence}");
        
        CheckWinCondition();
        return true;
    }
    
    private void UpdateThreadVisual()
    {
        // Clear existing segments
        ClearAllSegments();
        
        // Need at least 2 pins to draw anything
        if (_currentOrder.Count < 2) return;
        
        // Create a segment between each consecutive pair of pins
        for (int i = 0; i < _currentOrder.Count - 1; i++)
        {
            Pin startPin = _currentOrder[i];
            Pin endPin = _currentOrder[i + 1];
            
            CreateSegment(startPin, endPin);
        }
    }
    
    private void ClearAllSegments()
    {
        foreach (var segment in _currentSegments)
        {
            if (segment.GameObject != null)
                Destroy(segment.GameObject);
        }
        _currentSegments.Clear();
    }
    
    private void CreateSegment(Pin startPin, Pin endPin)
    {
        // Get world positions
        Vector3 startPos = startPin.ConnectionPoint != null ? 
            startPin.ConnectionPoint.position : startPin.transform.position;
        Vector3 endPos = endPin.ConnectionPoint != null ? 
            endPin.ConnectionPoint.position : endPin.transform.position;
        
        // Generate sag points
        List<Vector3> points = GenerateCatenaryPoints(startPos, endPos, _pointsPerSegment);
        
        // Calculate actual curved length of this segment
        float curvedLength = CalculateCurveLength(points);
        
        // Create GameObject for this segment
        GameObject segmentObj = new GameObject($"ThreadSegment_{startPin.PinID}_{endPin.PinID}");
        segmentObj.transform.SetParent(transform);
        
        // Add and configure LineRenderer
        LineRenderer line = segmentObj.AddComponent<LineRenderer>();
        line.startWidth = _threadWidth;
        line.endWidth = _threadWidth;
        line.material = new Material(_threadMaterial); // Create instance for per-segment tiling
        line.startColor = _normalColor;
        line.endColor = _normalColor;
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        
        // Apply world-space tiling based on actual curved length
        ApplyWorldSpaceTiling(line, curvedLength);
        
        // Store segment data
        _currentSegments.Add(new ThreadSegment
        {
            LineRenderer = line,
            GameObject = segmentObj,
            StartPin = startPin,
            EndPin = endPin,
            CachedLength = curvedLength
        });
    }
    
    private List<Vector3> GenerateCatenaryPoints(Vector3 start, Vector3 end, int resolution)
    {
        List<Vector3> points = new List<Vector3>(resolution + 1);
        
        // Calculate horizontal distance (ignoring Y axis for sag calculation)
        float horizontalDistance = Vector3.Distance(
            new Vector3(start.x, 0, start.z), 
            new Vector3(end.x, 0, end.z)
        );
        
        float maxSag = horizontalDistance * _sagFactor;
        
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            
            // Straight line position (includes height difference naturally)
            Vector3 point = Vector3.Lerp(start, end, t);
            
            // Calculate sag - maximum at t=0.5, zero at t=0 and t=1
            float sagCurve = 4 * t * (1 - t);
            float sagAmount = maxSag * sagCurve;
            
            // Sag pulls DOWN relative to the straight line (assumes Y is up axis)
            point.y -= sagAmount;
            
            points.Add(point);
        }
        
        return points;
    }
    
    private float CalculateCurveLength(List<Vector3> points)
    {
        float length = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }
        return length;
    }
    
    private void ApplyWorldSpaceTiling(LineRenderer line, float curvedLength)
    {
        // Calculate how many texture repeats based on actual curved length
        float tilingX = curvedLength * _tilingFrequency;
        
        // Apply tiling to the material instance
        line.material.mainTextureScale = new Vector2(tilingX, 1);
        line.material.mainTextureOffset = Vector2.zero;
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
        
        // Change all segment colors to gold
        foreach (var segment in _currentSegments)
        {
            if (segment.LineRenderer != null)
            {
                segment.LineRenderer.startColor = _winColor;
                segment.LineRenderer.endColor = _winColor;
            }
        }
    }
    
    public void ResetGame()
    {
        foreach (Pin pin in _currentOrder)
        {
            pin.SetInChain(false);
        }
        _currentOrder.Clear();
        ClearAllSegments();
        _gameWon = false;
        
        Debug.Log("Game reset");
    }
    
    public List<string> GetCurrentSequence()
    {
        return _currentOrder.Select(p => p.PinID).ToList();
    }
}