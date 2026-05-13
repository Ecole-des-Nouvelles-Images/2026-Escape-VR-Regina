using System;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [Header("===== Data =====")]
    public PuzzleData Data;
    public bool IsSolved {get; private set;}

    public virtual void Solve()
    {
        if (IsSolved)
            return;
        
        IsSolved = true;
        EventBus.OnPuzzleSolved?.Invoke(this);
    }

    public virtual string GetStringPuzzleHint(int index)
    {
        if (Data.PuzzleHints == null || Data.PuzzleHints.Count == 0)
            return "No String Hint Avaible! ";
        
        index = Mathf.Clamp(index,0,Data.PuzzleHints.Count - 1);
        return Data.PuzzleHints[index];
    }
    
    public virtual AudioClip GetSoundPuzzleHint(int index)
    {
        if (Data.PuzzleHintsSounds == null || Data.PuzzleHintsSounds.Count == 0)
            return null;
        
        index = Mathf.Clamp(index,0,Data.PuzzleHintsSounds.Count - 1);
        return Data.PuzzleHintsSounds[index];
    }
}
