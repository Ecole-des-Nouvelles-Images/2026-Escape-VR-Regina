using UnityEngine;
using System.Collections.Generic;

public class RotaryDialManager : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform _dialTransform;
    [SerializeField] private float _returnSpeed = 300f;       // degrees/sec
    [SerializeField] private float _returnDelay = 0.2f;       // seconds

    [Header("Detection")]
    [SerializeField] private LayerMask _fingerLayerMask = 1;   // Set to the layer of your fingertip collider
    [SerializeField] private Collider _outerZoneCollider;      // Large trigger covering the whole dial

    // State
    private enum State { Idle, Dialing, Returning }
    private State _currentState = State.Idle;

    // Dial geometry
    private Quaternion _initialLocalRotation;
    private float _currentRotationDelta = 0f;   // clamped 0..maxRotation

    // Tracking
    private Transform _trackedFinger;
    private HoleTrigger _currentHole;
    private float _initialAngle;                 // world angle of finger at entry (degrees)
    private float _holeMaxRotation;

    // Return delay timer
    private float _returnTimer = 0f;

    private void Start()
    {
        if (_dialTransform == null)
            Debug.LogError("RotaryDialManager: No dial transform assigned!", this);

        _initialLocalRotation = _dialTransform.localRotation;

        // Find all holes and subscribe to their events
        HoleTrigger[] holes = GetComponentsInChildren<HoleTrigger>();
        foreach (var hole in holes)
            hole.OnFingerEnter += OnFingerEnteredHole;

        // Subscribe to outer zone exit
        if (_outerZoneCollider != null)
        {
            var outerExit = _outerZoneCollider.GetComponent<OuterZoneExit>();
            if (outerExit == null)
                outerExit = _outerZoneCollider.gameObject.AddComponent<OuterZoneExit>();
            outerExit.OnFingerLeftZone += OnFingerLeftOuterZone;
        }
        else
        {
            Debug.LogWarning("RotaryDialManager: Outer zone collider not assigned – finger can exit without return.", this);
        }
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Dialing:
                UpdateDialing();
                break;
            case State.Returning:
                UpdateReturning();
                break;
        }
    }

    #region Core Mechanics

    private void UpdateDialing()
    {
        if (_trackedFinger == null)
        {
            // Lost finger reference – abort
            StartReturnSequence();
            return;
        }

        // Get current finger angle around dial center
        float currentAngle = GetAngleFromCenter(_trackedFinger.position);
        float rawDelta = currentAngle - _initialAngle;

        // Normalize to 0..360 range (clockwise only)
        if (rawDelta < 0f)
            rawDelta += 360f;

        // Clamp to hole's maximum
        float newDelta = Mathf.Clamp(rawDelta, 0f, _holeMaxRotation);

        if (!Mathf.Approximately(newDelta, _currentRotationDelta))
        {
            _currentRotationDelta = newDelta;
            ApplyRotation();
        }

        // (Optional: if you want to trigger return immediately when hitting the stop,
        //  uncomment the next line. The spec says release triggers return, so we don't.)
        // if (newDelta >= _holeMaxRotation && _currentState == State.Dialing) StartReturnSequence();
    }

    private void UpdateReturning()
    {
        if (_returnTimer > 0f)
        {
            _returnTimer -= Time.deltaTime;
            if (_returnTimer <= 0f)
                _returnTimer = 0f;
            return; // still waiting
        }

        // Rotate back toward 0°
        float step = _returnSpeed * Time.deltaTime;
        if (_currentRotationDelta <= step)
        {
            _currentRotationDelta = 0f;
            ApplyRotation();
            _currentState = State.Idle;
            _trackedFinger = null;
            _currentHole = null;
        }
        else
        {
            _currentRotationDelta -= step;
            ApplyRotation();
        }
    }

    private void StartReturnSequence()
    {
        if (_currentState == State.Returning)
            return;

        _currentState = State.Returning;
        _returnTimer = _returnDelay;
        _trackedFinger = null;
        _currentHole = null;
    }

    private void ApplyRotation()
    {
        // Clockwise rotation = negative angle around Z
        Quaternion deltaRot = Quaternion.Euler(0f, 0f, -_currentRotationDelta);
        _dialTransform.localRotation = _initialLocalRotation * deltaRot;
    }

    private float GetAngleFromCenter(Vector3 worldPoint)
    {
        Vector3 center = _dialTransform.position;
        Vector2 planePos = new Vector2(worldPoint.x - center.x, worldPoint.y - center.y);
        float angle = Mathf.Atan2(planePos.y, planePos.x) * Mathf.Rad2Deg;
        // Convert to 0..360 range for easier clockwise delta calculation
        if (angle < 0) angle += 360f;
        return angle;
    }

    #endregion

    #region Event Handlers

    private void OnFingerEnteredHole(HoleTrigger hole, Transform finger)
    {
        // Only allow new input if Idle
        if (_currentState != State.Idle) return;

        // Verify finger is on correct layer (optional)
        if (((1 << finger.gameObject.layer) & _fingerLayerMask) == 0) return;

        _currentState = State.Dialing;
        _trackedFinger = finger;
        _currentHole = hole;
        _holeMaxRotation = hole.MaxRotationDegrees;

        // Record initial finger angle
        _initialAngle = GetAngleFromCenter(finger.position);

        // Reset dial to zero before starting (ensures fresh start)
        _currentRotationDelta = 0f;
        ApplyRotation();

        Debug.Log($"Dialing started with hole {hole.name}, max {_holeMaxRotation}°");
    }

    private void OnFingerExitedHole(HoleTrigger hole, Transform finger)
    {
        // Only react if this was the tracked finger and we're currently dialing
        if (_currentState == State.Dialing && _trackedFinger == finger && _currentHole == hole)
        {
            StartReturnSequence();
        }
    }

    private void OnFingerLeftOuterZone(Transform finger)
    {
        // If the finger that left the outer zone is the one we're tracking while dialing, start return
        if (_currentState == State.Dialing && _trackedFinger == finger)
        {
            StartReturnSequence();
        }
    }

    #endregion

    // Optional: public reset method for debugging
    public void ResetDial()
    {
        _currentState = State.Idle;
        _currentRotationDelta = 0f;
        _returnTimer = 0f;
        _trackedFinger = null;
        _currentHole = null;
        ApplyRotation();
    }
}