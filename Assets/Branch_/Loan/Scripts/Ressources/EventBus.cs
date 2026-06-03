using System;
using UnityEngine;

public static class EventBus
{
    // =========================
    // GAME STATE EVENTS
    // =========================
    
    public static Action OnGameStarted;
    public static Action OnGamePaused;
    public static Action OnGameResumed;
    public static Action OnGameLoose;
    public static Action OnGameWin;
    
    // =========================
    // PUZZLE EVENTS
    // =========================
    
    public static Action OnPuzzleChanged;
    public static Action<Puzzle> OnPuzzleSolved;
    public static Action<int,int> OnResendCode;
    
    // =========================
    // Player EVENTS
    // =========================

    public static Action OnCloseEyes;
    public static Action OnOpenEyes;
}
