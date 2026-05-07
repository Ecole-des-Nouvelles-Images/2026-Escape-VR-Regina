using UnityEngine;
using System.Collections.Generic;

public class RotaryDialPush : MonoBehaviour
{
    [Header("Rotary Settings")]
    [SerializeField] private float _maxRotationAngle = 45f;
    [SerializeField] private float _springReturnSpeed = 8f;
    [SerializeField] private float _pokeSensitivity = 5f;
    
    [Header("Tracking Settings")]
    [SerializeField] private float _activationRadius = 0.1f;  // Distance to start tracking
    [SerializeField] private float _releaseRadius = 0.15f;   // Distance to release (should be > activation)
    [SerializeField] private LayerMask _pokePointLayers = -1;
    
    private float _currentRotation = 0f;
    private float _targetRotation = 0f;
    private Transform _currentPokePoint;
    private Vector3 _lastPokePosition;
    private bool _isPoking = false;
    
    // Cache for performance
    private Collider[] _nearbyColliders = new Collider[10];
    private Vector3 _dialCenter;
    
    private void Start()
    {
        // Cache dial center position (assuming this object rotates around its parent)
        _dialCenter = transform.parent != null ? transform.parent.position : transform.position;
    }
    
    private void Update()
    {
        // Update dial center in case it moves
        _dialCenter = transform.parent != null ? transform.parent.position : transform.position;
        
        if (!_isPoking)
        {
            // Try to find a new poke point
            TryStartPoking();
        }
        
        if (_isPoking && _currentPokePoint != null)
        {
            // Check if we should release
            float distanceToCenter = Vector3.Distance(_currentPokePoint.position, _dialCenter);
            if (distanceToCenter > _releaseRadius)
            {
                ReleaseDial();
            }
            else
            {
                // Track rotation
                UpdateRotation();
            }
        }
        else if (!_isPoking)
        {
            // Spring return
            _targetRotation = Mathf.Lerp(_targetRotation, 0f, Time.deltaTime * _springReturnSpeed);
            _currentRotation = Mathf.Lerp(_currentRotation, _targetRotation, Time.deltaTime * 15f);
        }
        
        // Apply rotation
        transform.localRotation = Quaternion.Euler(0f, _currentRotation, 0f);
    }
    
    private void TryStartPoking()
    {
        // Find all colliders within activation radius
        int hitCount = Physics.OverlapSphereNonAlloc(_dialCenter, _activationRadius, _nearbyColliders, _pokePointLayers);
        
        Transform closestPokePoint = null;
        float closestDistance = _activationRadius;
        
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _nearbyColliders[i];
            if (col != null && IsValidPokePoint(col))
            {
                float distance = Vector3.Distance(col.transform.position, _dialCenter);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPokePoint = col.transform;
                }
            }
        }
        
        if (closestPokePoint != null)
        {
            StartPoking(closestPokePoint);
        }
    }
    
    private void StartPoking(Transform pokePoint)
    {
        _currentPokePoint = pokePoint;
        _lastPokePosition = _currentPokePoint.position;
        _isPoking = true;
    }
    
    private void ReleaseDial()
    {
        _isPoking = false;
        _currentPokePoint = null;
        _targetRotation = 0f;
    }
    
    private void UpdateRotation()
    {
        Vector3 pokePosition = _currentPokePoint.position;
        
        Vector3 currentDirection = (pokePosition - _dialCenter).normalized;
        Vector3 lastDirection = (_lastPokePosition - _dialCenter).normalized;
        
        float angleDelta = Vector3.SignedAngle(lastDirection, currentDirection, transform.forward);
        
        float newTarget = _targetRotation + (angleDelta * _pokeSensitivity);
        newTarget = Mathf.Clamp(newTarget, 0f, _maxRotationAngle);
        
        if (newTarget >= 0f && newTarget <= _maxRotationAngle)
        {
            _targetRotation = newTarget;
        }
        
        _currentRotation = Mathf.Lerp(_currentRotation, _targetRotation, Time.deltaTime * 15f);
        _lastPokePosition = pokePosition;
        
        // Auto-release when reaching max rotation
        if (Mathf.Approximately(_targetRotation, _maxRotationAngle) || _targetRotation >= _maxRotationAngle)
        {
            ReleaseDial();
            _targetRotation = _maxRotationAngle;
        }
    }
    
    private bool IsValidPokePoint(Collider col)
    {
        return (_pokePointLayers.value & (1 << col.gameObject.layer)) != 0;
    }
    
    public void ResetDial()
    {
        _isPoking = false;
        _currentPokePoint = null;
        _targetRotation = 0f;
        _currentRotation = 0f;
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
    
    // Optional: Visualize radii in editor
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.parent != null ? transform.parent.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, _activationRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, _releaseRadius);
    }
}