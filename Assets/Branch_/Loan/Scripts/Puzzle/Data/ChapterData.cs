using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChapterData 
{
    [Header("===== Chapter Settings =====")]
    public int ChapterNumber;
    public float TimeToSwitch;
    
    [Header("===== Puzzle In Chapter =====")]
    public List<Puzzle> Puzzles;
    
    [Header("===== Sound In Chapter =====")]
    public List<AudioClip> Sounds;
}