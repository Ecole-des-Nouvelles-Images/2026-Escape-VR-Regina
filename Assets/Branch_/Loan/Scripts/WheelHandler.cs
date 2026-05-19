using System;
using TMPro;
using UnityEngine;

public class WheelHandler : MonoBehaviour
{
    [Header("===== References =====")]
    [SerializeField] private int _index;
    [SerializeField] private Transform _visual;
    
    [Header("===== Settings =====")]
    [SerializeField] private float _sensitivity = 5f;

    private float _currentAngle;
    private int _lastValue = -1;
    private int _currentValue;
    private Vector3 _lastFingerPosition;
    private Transform _activeFinger;
    private bool _isInteracting;
    private bool _isGrabbing;

    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!_isGrabbing)
            return;

        if (other.CompareTag("FingerTip"))
        {
            _activeFinger = other.transform;
            _lastFingerPosition = _activeFinger.position;
            _isInteracting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_isGrabbing)
            return;
        if (other.CompareTag("FingerTip"))
        {
            _isInteracting = false;
            _activeFinger = null;
            SnapToValue(); // Envoie le code uniquement ici au moment du lâcher
        }
    }
    #endregion

    private void Start()
    {
        EventBus.OnGrabLock += IsGrabLock;
        EventBus.OnReleaseLock += IsReleaseGrab;
    }

    private void OnDisable()
    {
        EventBus.OnGrabLock -= IsGrabLock;
        EventBus.OnReleaseLock -= IsReleaseGrab;
    }

    private void Update()
    {
        if (!_isInteracting || _activeFinger == null) return;

        Vector3 currentFingerPos = _activeFinger.position;
        Vector3 worldDelta = currentFingerPos - _lastFingerPosition;
        
        float localDeltaX = transform.InverseTransformDirection(worldDelta).x;

        // AJUSTEMENT 1 : Indépendance du framerate (basé sur ~90 FPS de référence pour garder ta sensibilité)
        float frameIndependentDelta = localDeltaX * (Time.deltaTime * 90f);

        // Rotation
        _currentAngle += frameIndependentDelta * _sensitivity * 360f;
        _visual.localRotation = Quaternion.Euler(0f, -_currentAngle, 0f);

        // Calcul de la valeur en continu (0 à 9)
        _currentValue = (10 - (Mathf.RoundToInt(Mathf.Repeat(_currentAngle, 360f) / 36f) % 10)) % 10;

        if (_currentValue != _lastValue)
        { 
            _lastValue = _currentValue;
        }
        
        _lastFingerPosition = currentFingerPos;
    }

    private void SnapToValue()
    {
        // AJUSTEMENT 2 : Sécurité pour s'assurer que _lastValue ne vaut pas -1 au premier clic
        if (_lastValue == -1) _lastValue = _currentValue;

        float snappedAngle = (10 - _lastValue) % 10 * 36f;
        _currentAngle = snappedAngle;
        _visual.localRotation = Quaternion.Euler(0f, -snappedAngle, 0f);
        
        // C'est bien ici et seulement ici que le code est envoyé
        EventBus.OnResendCode?.Invoke(_lastValue, _index);
    }

    private void IsGrabLock() => _isGrabbing = true;
    private void IsReleaseGrab() => _isGrabbing = false;
}