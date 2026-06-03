using UnityEngine;
using System.Collections.Generic;

public class RotaryDialManager : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform _dialTransform;
    [SerializeField] private float _returnSpeed = 300f;       // degrees/sec
    [SerializeField] private float _returnDelay = 0.6f;       // seconds (raised for Quest hand-tracking dropout tolerance)

    [Header("Detection")]
    [SerializeField] private LayerMask _fingerLayerMask = 1;   // Set to the layer of your fingertip collider
    [SerializeField] private Collider _outerZoneCollider;      // Large trigger covering the whole dial

    [Header("Gizmos Visualization")]
    [SerializeField] private bool _showGizmos = true;
    [SerializeField] private float _gizmoPlaneRadius = 1.0f;
    [SerializeField] private Color _gizmoPlaneColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color _gizmoFingerProjectionColor = Color.yellow;
    [SerializeField] private Color _gizmoAngleLineColor = Color.red;
    [SerializeField] private Color _gizmoAngleArcColor = new Color(1, 0, 0, 0.3f);

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
    
    // Quest hand-tracking jitter rejection
    private float _prevRawAngle;                 // raw angle from previous frame, for outlier rejection

    // For Gizmos - store finger position and projection info
    private Vector3 _lastFingerWorldPosition;
    private Vector3 _lastProjectedPoint;
    private float _lastCalculatedAngle;
    private float _lastAngleDelta;

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
        if (!_trackedFinger)
        {
            StartReturnSequence();
            return;
        }

        _lastFingerWorldPosition = _trackedFinger.position;
        float currentAngle = GetAngleFromCenter(_trackedFinger.position);
        _lastCalculatedAngle = currentAngle;

        // Calculate per-frame delta from the raw angle directly.
        // We do NOT smooth the angle itself — EMA smoothing scales down every
        // frame's contribution and causes the dial to require 2x the physical
        // rotation. Instead, we reject only implausible single-frame spikes.
        float rawDelta = currentAngle - _initialAngle;
    
        // Handle wrap-around (critical for crossing 0/360 boundary)
        if (rawDelta > 180f)
            rawDelta -= 360f;
        else if (rawDelta < -180f)
            rawDelta += 360f;

        // Outlier rejection: Quest occasionally emits a single rogue joint position.
        // If this frame's delta is more than 20° larger than last frame's, it is
        // almost certainly noise — skip it without advancing _initialAngle so the
        // spike is measured against the last good position next frame.
        float prevDelta = _prevRawAngle == 0f ? rawDelta : (currentAngle - _prevRawAngle);
        if (rawDelta > 180f) prevDelta -= 360f;
        else if (rawDelta < -180f) prevDelta += 360f;
        float frameJump = Mathf.Abs(rawDelta - prevDelta);
        _prevRawAngle = currentAngle;
        if (frameJump > 20f)
            return;

        // Advance the baseline so next frame measures from here.
        _initialAngle = currentAngle;

        float newDelta = _currentRotationDelta;
    
        // Minimum movement threshold — keeps truly stationary jitter from creeping
        // the dial, without blocking slow deliberate rotation.
        const float MIN_MOVEMENT_DEGREES = 1.0f;
    
        // Only respond to clockwise movement (ratchet: ignore backward motion)
        if (rawDelta > MIN_MOVEMENT_DEGREES)
        {
            // Accumulate and clamp to hole's max rotation
            float candidateDelta = Mathf.Min(_currentRotationDelta + rawDelta, _holeMaxRotation);
            newDelta = candidateDelta;
        }
    
        _lastAngleDelta = newDelta;

        if (!Mathf.Approximately(newDelta, _currentRotationDelta))
        {
            _currentRotationDelta = newDelta;
            ApplyRotation();
        }
    
        // Check if reached max rotation
        if (_currentRotationDelta >= _holeMaxRotation/2 - 0.1f && _currentState == State.Dialing)
        {
            StartReturnSequence();
        }
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
        Quaternion deltaRot = Quaternion.Euler(0f, 0f, _currentRotationDelta*2);
        _dialTransform.localRotation = _initialLocalRotation * deltaRot;
    }

    private float GetAngleFromCenter(Vector3 worldPoint)
    {
        // Get the dial's position and local axes
        Vector3 center = _dialTransform.position;
        Vector3 right = _dialTransform.right;   // X-axis of dial
        Vector3 up = _dialTransform.up;         // Y-axis of dial
        Vector3 forward = _dialTransform.forward; // Z-axis (normal to dial surface)
    
        // Vector from dial center to finger
        Vector3 toFinger = worldPoint - center;
    
        // Project onto dial's local XY plane (remove component along dial's normal)
        Vector3 projected = toFinger - Vector3.Project(toFinger, forward);

        // Guard: if the finger is at or very near the centre the angle is undefined;
        // return the last known angle to avoid a sudden jump.
        if (projected.magnitude < 0.01f)
            return _initialAngle;
        
        // Store projected point for Gizmos
        _lastProjectedPoint = center + projected;
    
        // Get local coordinates in dial's space
        float localX = Vector3.Dot(projected, right);
        float localY = Vector3.Dot(projected, up);
    
        // Calculate angle (0° = right axis, increasing counter-clockwise)
        float angle = Mathf.Atan2(localY, localX) * Mathf.Rad2Deg;
    
        // Convert to 0-360 range
        if (angle < 0) angle += 360f;
    
        return angle;
    }

    #endregion

    #region Event Handlers

    private void OnFingerEnteredHole(HoleTrigger hole, Transform finger)
    {
        // Guard against null/destroyed finger before touching any state
        if (finger == null) return;

        // Only allow new input if Idle
        if (_currentState != State.Idle) return;

        // Verify finger is on correct layer (optional)
        if (((1 << finger.gameObject.layer) & _fingerLayerMask) == 0) return;

        _currentState = State.Dialing;
        _trackedFinger = finger;
        _currentHole = hole;
        _holeMaxRotation = hole.MaxRotationDegrees;

        // Record initial finger angle and seed smoothing state
        _initialAngle = GetAngleFromCenter(finger.position);
        _prevRawAngle = _initialAngle;

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

    #region Gizmos Visualization

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        
        if (_dialTransform != null)
        {
            DrawDialPlane();
            DrawFingerProjection();
            DrawAngleVisualization();
        }
    }

    private void DrawDialPlane()
    {
        // Draw the XY plane of the dial
        Vector3 center = _dialTransform.position;
        Vector3 right = _dialTransform.right;
        Vector3 up = _dialTransform.up;
        
        // Draw a semi-transparent disc to represent the plane
        Gizmos.color = _gizmoPlaneColor;
        DrawDisc(center, _dialTransform.forward, _gizmoPlaneRadius);
        
        // Draw the plane axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + right * _gizmoPlaneRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + up * _gizmoPlaneRadius);
        
        // Draw the normal (forward) axis
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center, center + _dialTransform.forward * 0.5f);
        
        // Draw a wireframe circle to show the plane boundary
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        DrawWireCircle(center, _dialTransform.forward, _gizmoPlaneRadius);
    }

    private void DrawFingerProjection()
    {
        if (_currentState == State.Dialing && _trackedFinger != null)
        {
            // Draw the original finger position
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(_lastFingerWorldPosition, 0.025f);
            
            // Draw the projected point on the dial plane
            Gizmos.color = _gizmoFingerProjectionColor;
            Gizmos.DrawSphere(_lastProjectedPoint, 0.03f);
            
            // Draw a line from the finger to its projection
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(_lastFingerWorldPosition, _lastProjectedPoint);
            
            // Draw the vector from center to projected point
            Gizmos.color = _gizmoAngleLineColor;
            Gizmos.DrawLine(_dialTransform.position, _lastProjectedPoint);
        }
    }

    private void DrawAngleVisualization()
    {
        if (_currentState == State.Dialing && _trackedFinger != null)
        {
            Vector3 center = _dialTransform.position;
            Vector3 right = _dialTransform.right;
            
            // Calculate the direction of the reference angle (0°)
            Vector3 referenceDir = right;
            
            // Calculate the direction of the current finger angle
            Vector3 fingerDir = (_lastProjectedPoint - center).normalized;
            
            // Draw the reference line (0°)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + referenceDir * _gizmoPlaneRadius);
            
            // Draw the current angle line
            Gizmos.color = _gizmoAngleLineColor;
            Gizmos.DrawLine(center, center + fingerDir * _gizmoPlaneRadius);
            
            // Draw the angle arc
            DrawAngleArc(center, referenceDir, fingerDir, _gizmoPlaneRadius * 0.7f, _gizmoAngleArcColor);
            
            // Draw text label for angle (using Unity's Handles class - works in Scene view)
            #if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();
            Vector3 labelPos = center + (referenceDir + fingerDir).normalized * (_gizmoPlaneRadius * 0.8f);
            Vector3 screenPos = UnityEditor.HandleUtility.WorldToGUIPoint(labelPos);
            GUI.color = Color.red;
            GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 20, 100, 40), 
                      $"Angle: {_lastCalculatedAngle:F1}°\nDelta: {_lastAngleDelta:F1}°");
            UnityEditor.Handles.EndGUI();
            #endif
        }
    }

    private void DrawDisc(Vector3 center, Vector3 normal, float radius)
    {
        // Draw a filled disc by drawing multiple circles with decreasing radius
        int segments = 32;
        float step = 360f / segments;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * step * Mathf.Deg2Rad;
            Vector3 direction = Quaternion.LookRotation(normal) * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            Vector3 point = center + direction * radius;
            
            if (i > 0)
            {
                float prevAngle = (i - 1) * step * Mathf.Deg2Rad;
                Vector3 prevDirection = Quaternion.LookRotation(normal) * new Vector3(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle), 0);
                Vector3 prevPoint = center + prevDirection * radius;
                Gizmos.DrawLine(prevPoint, point);
            }
        }
        
        // Fill the disc with semi-transparent color
        Gizmos.color = new Color(_gizmoPlaneColor.r, _gizmoPlaneColor.g, _gizmoPlaneColor.b, 0.1f);
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * step * Mathf.Deg2Rad;
            float angle2 = (i + 1) * step * Mathf.Deg2Rad;
            
            Vector3 dir1 = Quaternion.LookRotation(normal) * new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0);
            Vector3 dir2 = Quaternion.LookRotation(normal) * new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0);
            
            Vector3 p1 = center + dir1 * radius;
            Vector3 p2 = center + dir2 * radius;
            
            // Draw triangle fan from center
            Gizmos.DrawLine(center, p1);
            Gizmos.DrawLine(center, p2);
            Gizmos.DrawLine(p1, p2);
        }
    }

    private void DrawWireCircle(Vector3 center, Vector3 normal, float radius)
    {
        int segments = 64;
        float step = 360f / segments;
        
        Vector3 prevPoint = center + Quaternion.LookRotation(normal) * new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step * Mathf.Deg2Rad;
            Vector3 direction = Quaternion.LookRotation(normal) * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            Vector3 point = center + direction * radius;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    private void DrawAngleArc(Vector3 center, Vector3 fromDir, Vector3 toDir, float radius, Color color)
    {
        Gizmos.color = color;
        
        // Calculate the angle between the two directions
        float angle = Vector3.SignedAngle(fromDir, toDir, _dialTransform.forward);
        if (angle < 0) angle += 360;
        
        // Draw the arc using line segments
        int segments = Mathf.Max(10, Mathf.CeilToInt(angle / 5f));
        float step = angle / segments;
        
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = i * step;
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, _dialTransform.forward);
            Vector3 direction = rotation * fromDir;
            Vector3 point = center + direction * radius;
            
            if (i > 0)
            {
                float prevAngle = (i - 1) * step;
                Quaternion prevRotation = Quaternion.AngleAxis(prevAngle, _dialTransform.forward);
                Vector3 prevDirection = prevRotation * fromDir;
                Vector3 prevPoint = center + prevDirection * radius;
                Gizmos.DrawLine(prevPoint, point);
            }
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