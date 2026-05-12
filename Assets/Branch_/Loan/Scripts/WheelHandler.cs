using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WheelHandler : MonoBehaviour
{
    [Header("===== Wheel Settings =====")]

    [SerializeField] private int _index;

    [SerializeField] private Transform _visual;

    [SerializeField] private LockManager _lockManager;

    [SerializeField] private float _rotationSpeed = 250f;

    [SerializeField]private XRBaseInteractor _currentInteractor;

    [SerializeField] private XRSimpleInteractable _simpleInteractable;

    private float _currentAngle;

    private int _value;

    private Vector3 _lastInteractorPosition;

    private bool _isInteracting;

    private void Awake()
    {
        _simpleInteractable = GetComponent<XRSimpleInteractable>();

        _simpleInteractable.hoverEntered.AddListener(OnHoverEntered);
        _simpleInteractable.hoverExited.AddListener(OnHoverExited);
    }

    private void Update()
    {
        if (!_isInteracting || _currentInteractor == null)
            return;

        Vector3 currentPosition = _currentInteractor.transform.position;

        Vector3 delta = currentPosition - _lastInteractorPosition;

        _currentAngle += delta.x * _rotationSpeed;

        _currentAngle = Mathf.Repeat(_currentAngle, 360f);

        _value = Mathf.RoundToInt(_currentAngle / 36f) % 10;

        float snappedAngle = _value * 36f;

        _visual.localRotation = Quaternion.Euler(0f, snappedAngle, 0f);

        _lockManager.GetValue(_value, _index);

        _lastInteractorPosition = currentPosition;
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        _currentInteractor = args.interactorObject as XRBaseInteractor;

        if (_currentInteractor == null)
            return;

        _lastInteractorPosition = _currentInteractor.transform.position;

        _isInteracting = true;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        _isInteracting = false;

        _currentInteractor = null;

        float snappedAngle = _value * 36f;

        _visual.localRotation = Quaternion.Euler(0f, snappedAngle, 0f);
    }
}