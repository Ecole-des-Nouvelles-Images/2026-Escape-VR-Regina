using System;
using System.Collections;
using UnityEngine;


public class PuzzleSequenceManager : MonoBehaviour
{
    public static PuzzleSequenceManager Instance;
    
    [Header("===== Ordered Puzzles =====")]
    public ChapterData CurrentChapter;
    [SerializeField] private int _currentChapterIndex;
    [SerializeField] private int _currentPuzzleIndex = 0;
    [SerializeField] private int _currentHintIndex = 0;
    private CameraHandler _cameraHandler;
    private SceneTransitionManager _sceneTransitionManager;
    public Puzzle CurrentPuzzle => CurrentChapter.Puzzles[_currentPuzzleIndex];
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
       
        
        _currentChapterIndex = 0;
        
        _cameraHandler = FindAnyObjectByType<CameraHandler>();
        _sceneTransitionManager = FindAnyObjectByType<SceneTransitionManager>();
    }

    private void Start()
    {
        EventBus.OnPuzzleSolved += HandlePuzzleSolved;
        EventBus.OnGameLoose += ResetManager;
        // EventBus.OnGameStarted += ResetManager;
    }

    private void OnDestroy()
    {
        EventBus.OnPuzzleSolved -= HandlePuzzleSolved;
        EventBus.OnGameLoose -= ResetManager;
        // EventBus.OnGameStarted -= ResetManager;
    }
    
    public void InjectCurrentChapter(ChapterData chapter)
    {
        CurrentChapter = chapter;
        _currentChapterIndex++;
        _currentPuzzleIndex = 0;
        
        Debug.Log($"Nouveau chapitre injecté : {chapter.ChapterNumber}");
        StartPuzzle();
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

        if (_currentPuzzleIndex >= CurrentChapter.Puzzles.Count)
        {
            Debug.Log("All puzzles completed!");
            StartCoroutine(SceneSwitching(CurrentChapter.TimeToSwitch));
            CurrentChapter = null;
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

    private void ResetManager()
    {
        CurrentChapter = null;
        //_currentChapterIndex++;
        _currentPuzzleIndex = 0;
        _currentHintIndex = 0;
    }
    
    private IEnumerator SceneSwitching(float time)
    {
        yield return new WaitForSeconds(time);
        _sceneTransitionManager.LoadActScene(_currentChapterIndex + 1);
    }
}