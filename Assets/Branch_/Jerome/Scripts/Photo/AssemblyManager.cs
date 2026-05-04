using UnityEngine;

public class AssemblyManager : MonoBehaviour
{
    public int totalPieces; // How many pieces needed
    public GameObject reconstructedObject; // The final object
    
    private int piecesPlaced = 0;
    
    void Start()
    {
        reconstructedObject.SetActive(false);
    }
    
    public void PiecePlaced()
    {
        piecesPlaced++;
        
        // if (piecesPlaced >= totalPieces)
        // {
        //     // Hide all pieces (optional - they're already snapped)
        //     // Show reconstructed object
        //     reconstructedObject.SetActive(true);
        // }
    }
}