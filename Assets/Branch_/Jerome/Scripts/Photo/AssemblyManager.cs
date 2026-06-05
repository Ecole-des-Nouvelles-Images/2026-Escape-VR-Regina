using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class AssemblyManager : Puzzle
{
    [SerializeField] private GameObject _reconstructedObject;
    [SerializeField] private float _completionDelay = 0.5f; // Brief pause before reveal

    private int _totalPieces;
    private int _piecesPlaced = 0;
    private readonly List<GameObject> _snappedPieces = new();

    private void Start()
    {
        _reconstructedObject.SetActive(false);
        _totalPieces = FindObjectsByType<SocketSnapHandler>(FindObjectsSortMode.None).Length;
        Debug.Log($"AssemblyManager ready. Expecting {_totalPieces} pieces.");
    }

    public void PiecePlaced(GameObject snappedPiece, SocketSnapHandler handler)
    {
        if (_snappedPieces.Contains(snappedPiece)) return;

        _snappedPieces.Add(snappedPiece);
        _piecesPlaced++;
        Debug.Log($"Piece {snappedPiece.name} snapped. {_piecesPlaced}/{_totalPieces} placed.");

        if (_piecesPlaced >= _totalPieces)
            StartCoroutine(CompleteAssembly());
    }

    private IEnumerator CompleteAssembly()
    {
        Debug.Log("All pieces placed! Completing assembly...");

        yield return new WaitForSeconds(_completionDelay);

        // Destroy all snapped pieces and reveal the reconstructed object
        foreach (GameObject piece in _snappedPieces.Where(p => p))
            Destroy(piece);

        _snappedPieces.Clear();

        if (_reconstructedObject)
            _reconstructedObject.SetActive(true);

        Solve();
        Debug.Log("Assembly complete! Final object revealed.");
    }
}