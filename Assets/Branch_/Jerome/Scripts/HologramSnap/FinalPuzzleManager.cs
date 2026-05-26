using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FinalPuzzleManager : Puzzle
{
    [Header("Win/Lose Effects")]
    [SerializeField] private GameObject _successObject; // Object to activate on success
    [SerializeField] private GameObject _failureEffect; // Optional: effect to show on failure
    
    private int _requiredCorrectPieces;
    private int _correctPiecesPlaced = 0;
    private readonly List<GameObject> _correctPieces = new();
    private readonly List<HologramSnapManager> _socketManagers = new();
    private bool _puzzleComplete = false;
    
    private void Start()
    {
        // Find all hologram snap managers
        HologramSnapManager[] managers = FindObjectsByType<HologramSnapManager>(FindObjectsSortMode.None);
        _socketManagers.AddRange(managers);
        
        if (_successObject != null)
            _successObject.SetActive(false);
        
        _requiredCorrectPieces = FindObjectsByType<HologramSnapManager>(FindObjectsSortMode.None).Length;
    }
    
    public void OnCorrectPiecePlaced(GameObject piece, HologramSnapManager manager)
    {
        if (_puzzleComplete) return;
        
        if (!_correctPieces.Contains(piece))
        {
            _correctPieces.Add(piece);
            _correctPiecesPlaced++;
            Debug.Log($"Correct piece placed: {piece.name}. {_correctPiecesPlaced}/{_requiredCorrectPieces}");
        }
        
        // Check if puzzle is complete
        if (_correctPiecesPlaced >= _requiredCorrectPieces)
        {
            OnPuzzleSuccess();
        }
    }
    
    public void OnWrongPiecePlaced(GameObject piece, HologramSnapManager manager)
    {
        if (_puzzleComplete) return;
        
        Debug.Log($"Wrong piece placed: {piece.name}");
        OnPuzzleFailure(piece);
    }
    
    private void OnPuzzleSuccess()
    {
        _puzzleComplete = true;
        Debug.Log("Puzzle completed successfully!");
        
        if (_successObject != null)
            _successObject.SetActive(true);
        
        // Call your success method here
        OnSuccess();
    }
    
    private void OnPuzzleFailure(GameObject wrongPiece)
    {
        Debug.Log($"Puzzle failure triggered by: {wrongPiece.name}");
        
        // Play failure effect if assigned
        if (_failureEffect != null)
        {
            Instantiate(_failureEffect, wrongPiece.transform.position, Quaternion.identity);
        }
        
        // Call your failure method here
        OnFailure(wrongPiece);
        
        // Optional: Reset all sockets and pieces
        ResetAllSockets();
    }
    
    private void ResetAllSockets()
    {
        foreach (HologramSnapManager manager in _socketManagers)
        {
            GameObject piece = manager.GetSnappedPiece();
            if (piece != null)
            {
                // Reactivate piece
                XRGrabInteractable grab = piece.GetComponent<XRGrabInteractable>();
                if (grab != null) grab.enabled = true;
                
                Rigidbody rb = piece.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }
            
            manager.ResetSocket();
        }
        
        _correctPieces.Clear();
        _correctPiecesPlaced = 0;
    }
    
    private void OnSuccess()
    {
        // If there's more to happen before saying that it has been solved
        
        Solve();
    }
    
    private void OnFailure(GameObject wrongPiece)
    {
        // TODO: Implement your failure logic here
    }
}