using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotaryDialManager : Puzzle
{
    [Tooltip("7 digit numbers that solve the puzzle")]
    [SerializeField] private List<string> _validNumbers = new() { "1234567", "7654321" };

    [Header("Vibration Integration")]
    [Tooltip("Reference to the rotary handle that should vibrate")]
    [SerializeField] private Transform _phoneHandle;

    [Tooltip("How long incoming calls should vibrate")]
    [SerializeField] private float _incomingCallVibrationDuration = 3f;

    [Tooltip("Whether to use the wrong number vibration pattern")]
    [SerializeField] private bool _useWrongNumberPattern = true;

    [Header("Dial Reference")]
    [Tooltip("The RotaryDialController on the phone dial")]
    [SerializeField] private RotaryDialController _dialController;

    private readonly List<char> _phoneNumber = new();
    private RotaryHandleVibrator _vibrator;
    private bool _hasIncomingCall = false;

    private void Start()
    {
        SetupVibrator();
    }

    private void SetupVibrator()
    {
        if (_phoneHandle == null)
        {
            Debug.LogWarning("RotaryPhoneInputHandler: Phone handle not assigned — vibration will not work.", this);
            return;
        }

        _vibrator = _phoneHandle.GetComponent<RotaryHandleVibrator>();
        if (_vibrator == null)
            _vibrator = _phoneHandle.gameObject.AddComponent<RotaryHandleVibrator>();
    }

    private void SubscribeToDialManager()
    {
        if (_dialController == null)
        {
            // Fall back to searching on the same GameObject or children
            _dialController = GetComponentInChildren<RotaryDialController>();
        }

        if (_dialController == null)
        {
            Debug.LogError("RotaryPhoneInputHandler: No RotaryDialManager found — digit input will not work.", this);
            return;
        }

        _dialController.OnDigitDialled += OnDigitDialled;
    }

    /// <summary>
    /// Called by RotaryDialManager each time a hole has been fully dialled.
    /// </summary>
    private void OnDigitDialled(string holeName)
    {
        if (holeName.Length != 1 || !char.IsDigit(holeName[0]))
        {
            Debug.LogWarning($"RotaryPhoneInputHandler: Received unexpected hole name '{holeName}', ignoring.", this);
            return;
        }

        _phoneNumber.Add(holeName[0]);
        Debug.Log($"Digit dialled: {holeName[0]}  —  sequence so far: {new string(_phoneNumber.ToArray())}");

        ValidateCurrentSequence();
    }

    /// <summary>
    /// Validates the accumulated digit sequence against the list of valid numbers.
    /// </summary>
    private void ValidateCurrentSequence()
    {
        if (_phoneNumber.Count == 0) return;

        string current = new(_phoneNumber.ToArray());

        // Full match
        if (_validNumbers.Contains(current))
        {
            Solve();
            _phoneNumber.Clear();
            return;
        }

        // Still a valid prefix of at least one number — keep going
        if (_validNumbers.Any(number => number.StartsWith(current))) return;

        // No match and no valid prefix — wrong number
        WrongNumber();
        _phoneNumber.Clear();
    }

    private void WrongNumber()
    {
        Debug.Log("RotaryPhoneInputHandler: Wrong number — resetting sequence.", this);

        if (_vibrator != null)
        {
            if (_useWrongNumberPattern)
                _vibrator.WrongNumber();
            else
                _vibrator.StartVibration(0.3f);
        }

        if (_hasIncomingCall)
        {
            CancelInvoke(nameof(StopIncomingCall));
            StopIncomingCall();
        }
    }

    public override void Solve()
    {
        if (_hasIncomingCall)
            StopIncomingCall();

        base.Solve();
        Debug.Log("RotaryPhoneInputHandler: Puzzle solved — correct number entered.", this);
    }

    private void StartIncomingCall()
    {
        if (_hasIncomingCall) return;

        _hasIncomingCall = true;

        if (_vibrator != null)
        {
            _vibrator.StartVibration(_incomingCallVibrationDuration);
            Debug.Log("RotaryPhoneInputHandler: Phone ringing — handle vibrating.", this);
        }

        Invoke(nameof(StopIncomingCall), _incomingCallVibrationDuration);
    }

    private void StopIncomingCall()
    {
        _hasIncomingCall = false;

        if (_vibrator != null && _vibrator.IsVibrating())
            _vibrator.StopVibration();
    }

    private void OnEnable()
    {
        EventBus.OnPuzzleSolved += OnPuzzleSolved;
    }

    private void OnDisable()
    {
        EventBus.OnPuzzleSolved -= OnPuzzleSolved;

        if (_dialController != null)
            _dialController.OnDigitDialled -= OnDigitDialled;

        if (_vibrator != null && _vibrator.IsVibrating())
            _vibrator.StopVibration();
    }

    private void OnDestroy()
    {
        if (_dialController != null)
            _dialController.OnDigitDialled -= OnDigitDialled;
    }

    private void OnPuzzleSolved(Puzzle puzzle)
    {
        if (puzzle.PuzzleID != 1) return;

        SubscribeToDialManager();
        StartIncomingCall();
    }
}