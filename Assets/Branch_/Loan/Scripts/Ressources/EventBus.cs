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
    // public static Action OnHintGived;
    public static Action OnGrabLock;
    public static Action OnReleaseLock;
    public static Action<int,int> OnResendCode;
    // public static Action OnOpenChest;
    
    // =========================
    // PLAYER EVENTS
    // =========================

    public static Action OnCloseEyes;
    public static Action OnOpenEyes;
}
