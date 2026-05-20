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
    
    // =========================
    // PUZZLE EVENTS
    // =========================
    
    public static Action OnPuzzleChanged;
    public static Action<Puzzle> OnPuzzleSolved;
    public static Action<int,int> OnResendCode;
    
    // =========================
    // PLAYER EVENTS
    // =========================
    
}
