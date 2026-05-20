using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSequenceManager : MonoBehaviour
{
    public static PuzzleSequenceManager Instance;
    
    [Header("===== Ordered Puzzles =====")]
    [SerializeField] private List<Puzzle> _puzzles;

    [SerializeField] private int _currentPuzzleIndex = 0;
    [SerializeField] private int _currentHintIndex = 0;
    
    public Puzzle CurrentPuzzle => _puzzles[_currentPuzzleIndex];
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        EventBus.OnPuzzleSolved += HandlePuzzleSolved;
        StartPuzzle();
    }

    private void OnDestroy()
    {
        EventBus.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void StartPuzzle()
    {
        _currentHintIndex = 0;
        EventBus.OnPuzzleChanged?.Invoke();
        
        Debug.Log($"Starting puzzle: {CurrentPuzzle.Data.PuzzleName}");
    }
    
    private void HandlePuzzleSolved(Puzzle puzzle)
    {
        if (puzzle != CurrentPuzzle)
            return;

        Debug.Log($"Solved: {puzzle.Data.PuzzleName}");

        NextPuzzle();
    }
    
    private void NextPuzzle()
    {
        _currentPuzzleIndex++;

        if (_currentPuzzleIndex >= _puzzles.Count)
        {
            Debug.Log("All puzzles completed!");
            EventBus.OnCloseEyes?.Invoke();
            return;
        }

        StartPuzzle();
    }

    public string GiveStringHint()
    {
        if (CurrentPuzzle == null)
            return "No String active puzzle.";

        string hint = CurrentPuzzle.GetStringPuzzleHint(_currentHintIndex);
        _currentHintIndex++;

        return hint;
    }

    public AudioClip GiveAudioClipHint()
    {
        if (CurrentPuzzle == null)
            return null;

        AudioClip hint = CurrentPuzzle.GetSoundPuzzleHint(_currentHintIndex);
        _currentHintIndex++;

        return hint;
    }
}
