using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AssemblyManager : Puzzle
{
    [SerializeField] private GameObject _reconstructedObject; // The final object
    
    private int _totalPieces; // How many pieces needed
    private int _piecesPlaced = 0;
    private readonly List<GameObject> _snappedPieces = new(); // Track snapped pieces
    private readonly List<SocketSnapHandler> _socketHandlers = new(); // Track socket handlers

    private void Start()
    {
        _reconstructedObject.SetActive(false);
        
        // Find all socket handlers
        SocketSnapHandler[] handlers = FindObjectsByType<SocketSnapHandler>(FindObjectsSortMode.None);
        _totalPieces = handlers.Length;
        
        // Store references to all socket handlers
        _socketHandlers.Clear();
        _socketHandlers.AddRange(handlers);
    }
    
    public void PiecePlaced(GameObject snappedPiece, SocketSnapHandler handler)
    {
        // Add to tracking list if not already there
        if (!_snappedPieces.Contains(snappedPiece))
        {
            _snappedPieces.Add(snappedPiece);
            _piecesPlaced++;
            Debug.Log($"Piece {snappedPiece.name} snapped. {_piecesPlaced}/{_totalPieces} pieces placed.");
        }
        
        // Check if all pieces are in place
        if (_piecesPlaced >= _totalPieces)
        {
            CompleteAssembly();
        }
    }

    private void CompleteAssembly()
    {
        Debug.Log("All pieces assembled! Completing assembly...");
        
        // Call CompletedPuzzle on all socket handlers to animate each piece
        foreach (SocketSnapHandler handler in _socketHandlers.Where(handler => handler)) handler.CompletedPuzzle();
        
        // Wait for animations to complete before showing the reconstructed object
        StartCoroutine(WaitForAnimationsAndComplete());
    }
    
    private System.Collections.IEnumerator WaitForAnimationsAndComplete()
    {
        // Wait for all animations to complete (adjust timing as needed)
        // Since we don't have a direct way to check all animations, we'll use a delay
        
        const float maxAnimationTime = 1f; // Same as lerp duration
        yield return new WaitForSeconds(maxAnimationTime + 0.1f); // Small buffer
        
        // Remove all snapped pieces
        foreach (GameObject piece in _snappedPieces.Where(piece => piece)) Destroy(piece);
        
        // Clear the list
        _snappedPieces.Clear();
        
        // Show reconstructed object
        if (_reconstructedObject) _reconstructedObject.SetActive(true);
        
        Solve();
        Debug.Log("Assembly complete! Final object revealed.");
    }
}