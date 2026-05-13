using System;
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
    private Vector3 _lastFingerPosition;
    private Transform _activeFinger;
    private bool _isInteracting;
    private bool _isGrabbing;

    #region Trigger
//===================================================================================================================================================================================================
    private void OnTriggerEnter(Collider other)
    {
        if (!_isGrabbing)
            return;
        // On vérifie si c'est bien le bout du doigt qui touche
        if (other.CompareTag("FingerTip"))
        {
            _activeFinger = other.transform;
            _lastFingerPosition = _activeFinger.position;
            _isInteracting = true;
            Debug.Log($"Wheel {_index} : Contact avec le doigt !");
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
            SnapToValue();
            Debug.Log($"Wheel {_index} : Fin de contact.");
        }
    }
//===================================================================================================================================================================================================
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

        // Calcul du déplacement latéral
        Vector3 currentFingerPos = _activeFinger.position;
        Vector3 worldDelta = currentFingerPos - _lastFingerPosition;
        
        // On projette sur l'axe X local de la molette
        float localDeltaX = transform.InverseTransformDirection(worldDelta).x;

        // Rotation
        _currentAngle += localDeltaX * _sensitivity * 360f;
        _visual.localRotation = Quaternion.Euler(0f, -_currentAngle, 0f);

        // Calcul de la valeur
        int currentValue = Mathf.RoundToInt(Mathf.Repeat(_currentAngle, 360f) / 36f) % 10;
        
        if (currentValue != _lastValue)
        { 
            _lastValue = currentValue;
            
           EventBus.OnResendCode?.Invoke(currentValue, _index);
        }

        _lastFingerPosition = currentFingerPos;
    }

    private void SnapToValue()
    {
        float snappedAngle = _lastValue * 36f;
        _currentAngle = snappedAngle;
        _visual.localRotation = Quaternion.Euler(0f, snappedAngle, 0f);
    }

    private void IsGrabLock()
    {
        _isGrabbing = true;
    }

    private void IsReleaseGrab()
    {
        _isGrabbing = false;
    }
}