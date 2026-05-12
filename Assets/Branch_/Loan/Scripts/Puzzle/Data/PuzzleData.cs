using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Scriptable Objects/PuzzleData")]
public class PuzzleData : ScriptableObject
{
    public string PuzzleName;
    
    [TextArea]
    public string PuzzleDescription;
    
    [TextArea]
    public List<string> PuzzleHints;
    
    public List<AudioClip> PuzzleHintsSounds;
}
