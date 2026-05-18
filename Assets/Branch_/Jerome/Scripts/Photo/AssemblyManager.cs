using UnityEngine;
using System.Collections.Generic;

public class AssemblyManager : MonoBehaviour
{
    [SerializeField] private GameObject _reconstructedObject; // The final object
    
    private int _totalPieces; // How many pieces needed

    public int PiecesPlaced = 0;
    private readonly List<GameObject> _snappedPieces = new(); // Track snapped pieces

    private void Start()
    {
        _reconstructedObject.SetActive(false);

        _totalPieces = FindObjectsByType<SocketSnapHandler>(FindObjectsSortMode.None).Length;
    }
    
    public void PiecePlaced(GameObject snappedPiece)
    {
        // Add to tracking list if not already there
        if (!_snappedPieces.Contains(snappedPiece))
        {
            _snappedPieces.Add(snappedPiece);
            PiecesPlaced++;
            Debug.Log($"Piece {snappedPiece.name} snapped. {PiecesPlaced}/{_totalPieces} pieces placed.");
        }
        
        // Check if all pieces are in place
        if (PiecesPlaced >= _totalPieces)
        {
            CompleteAssembly();
        }
    }
    
    void CompleteAssembly()
    {
        Debug.Log("All pieces assembled! Completing assembly...");
        
        // Remove all snapped pieces
        foreach (GameObject piece in _snappedPieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }
        
        // Clear the list
        _snappedPieces.Clear();
        
        // Show reconstructed object
        _reconstructedObject.SetActive(true);
        
        Debug.Log("Assembly complete! Final object revealed.");
    }
}