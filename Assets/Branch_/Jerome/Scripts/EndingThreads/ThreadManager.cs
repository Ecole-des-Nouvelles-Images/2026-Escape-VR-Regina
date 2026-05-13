using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    [Header("Thread Visual")]
    public LineRenderer threadLine;
    public Material threadMaterial;
    public float threadWidth = 0.02f;
    
    [Header("Win Condition")]
    public List<string> targetSequence; // Set in Inspector: ["A","C","B","D","F"]
    
    [Header("References")]
    public Transform threadStartPoint; // Where thread originates (spool or hand)
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attachSound;
    public AudioClip removeSound;
    public AudioClip winSound;
    
    // Runtime data
    private List<Pin> currentOrder = new List<Pin>();
    private bool gameWon = false;
    
    void Start()
    {
        if (threadLine == null)
            threadLine = GetComponent<LineRenderer>();
            
        if (threadLine != null)
        {
            threadLine.startWidth = threadWidth;
            threadLine.endWidth = threadWidth;
            threadLine.material = threadMaterial;
            threadLine.positionCount = 0;
        }
    }
    
    public bool AddPin(Pin newPin)
    {
        if (gameWon) return false;
        
        // Prevent adding same pin twice
        if (currentOrder.Contains(newPin))
        {
            Debug.Log($"Pin {newPin.pinID} already in chain");
            return false;
        }
        
        // Add to chain
        currentOrder.Add(newPin);
        newPin.SetInChain(true);
        newPin.PlayAttachEffect();
        
        // Play sound
        if (audioSource != null && attachSound != null)
            audioSource.PlayOneShot(attachSound);
        
        // Update visual thread
        UpdateThreadVisual();
        
        // Log current sequence
        string sequence = currentOrder.Count > 0 ? 
            string.Join(" → ", currentOrder.Select(p => p.pinID)) : "Empty";
        Debug.Log($"Current sequence: {sequence}");
        
        // Check win condition
        CheckWinCondition();
        
        return true;
    }
    
    public bool RemovePin(Pin targetPin)
    {
        if (gameWon) return false;
        
        int index = currentOrder.IndexOf(targetPin);
        if (index == -1)
        {
            Debug.Log($"Pin {targetPin.pinID} not in chain");
            return false;
        }
        
        // Remove from chain
        currentOrder.RemoveAt(index);
        targetPin.SetInChain(false);
        
        // Play sound
        if (audioSource != null && removeSound != null)
            audioSource.PlayOneShot(removeSound);
        
        // Update visual thread (automatically reconnects neighbors)
        UpdateThreadVisual();
        
        // Log new sequence
        string sequence = currentOrder.Count > 0 ? 
            string.Join(" → ", currentOrder.Select(p => p.pinID)) : "Empty";
        Debug.Log($"After removal: {sequence}");
        
        return true;
    }
    
    void UpdateThreadVisual()
    {
        if (threadLine == null) return;
        
        // Need at least 2 points to draw a line
        if (currentOrder.Count < 2)
        {
            threadLine.positionCount = 0;
            return;
        }
        
        // Build positions list
        List<Vector3> positions = new List<Vector3>();
        
        // Optional: Add thread start point (spool)
        if (threadStartPoint != null)
            positions.Add(threadStartPoint.position);
        
        // Add each pin's connection point
        foreach (Pin pin in currentOrder)
        {
            if (pin.connectionPoint != null)
                positions.Add(pin.connectionPoint.position);
            else
                positions.Add(pin.transform.position);
        }
        
        // Update line renderer
        threadLine.positionCount = positions.Count;
        threadLine.SetPositions(positions.ToArray());
    }
    
    void CheckWinCondition()
    {
        // Convert current pins to IDs
        List<string> currentIDs = currentOrder.Select(p => p.pinID).ToList();
        
        // Check if sequences match exactly
        bool sequenceMatches = currentIDs.SequenceEqual(targetSequence);
        
        if (sequenceMatches && currentIDs.Count == targetSequence.Count)
        {
            Win();
        }
    }
    
    void Win()
    {
        gameWon = true;
        Debug.Log("🎉 VICTORY! Correct sequence achieved! 🎉");
        
        if (audioSource != null && winSound != null)
            audioSource.PlayOneShot(winSound);
            
        // Change thread color to gold
        if (threadLine != null)
            threadLine.startColor = Color.yellow;
            threadLine.endColor = Color.yellow;
            
        // You can add more win effects here:
        // - Particle effects
        // - UI panel
        // - Load next scene
    }
    
    // Debug/Editor method to reset the game
    public void ResetGame()
    {
        foreach (Pin pin in currentOrder)
        {
            pin.SetInChain(false);
        }
        currentOrder.Clear();
        UpdateThreadVisual();
        gameWon = false;
        
        if (threadLine != null && threadMaterial != null)
        {
            threadLine.startColor = Color.white;
            threadLine.endColor = Color.white;
            threadLine.material = threadMaterial;
        }
            
        Debug.Log("Game reset");
    }
    
    // Optional: Get current sequence for UI display
    public List<string> GetCurrentSequence()
    {
        return currentOrder.Select(p => p.pinID).ToList();
    }
}