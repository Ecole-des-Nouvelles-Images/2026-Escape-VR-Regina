using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LockManager : Puzzle
{
    [Header("===== Puzzle Settings =====")]

    [SerializeField] private List<int> _values = new();
    [SerializeField] private List<int> _codes = new();

    [Header("===== Inspect Settings =====")]

    [SerializeField] private Transform _inspectPoint;
    [SerializeField] private float _inspectScaleMultiplier = 2f;
    [SerializeField] private bool _useStartPos;

    private XRGrabInteractable _grabInteractable;

    private Rigidbody _rb;

    [SerializeField]private Vector3 _startPosition;
    [SerializeField]private Quaternion _startRotation;
    [SerializeField]private Vector3 _startScale;
    [SerializeField]private Transform _startParent;

    private bool _isInspecting;
    Transform _thisTransform;

    private void Start()
    {
        EventBus.OnResendCode += GetValue;
        while (_values.Count < _codes.Count)
        {
            _values.Add(0);
        }

        while (_values.Count > _codes.Count)
        {
            _values.RemoveAt(_values.Count - 1);
        }

        _grabInteractable = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();

        _thisTransform = GetComponent<Transform>();
        _grabInteractable.selectEntered.AddListener(OnGrab);
        _grabInteractable.selectExited.AddListener(OnRelease);

        _startScale = transform.localScale;
        
        if (_useStartPos)
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startParent = transform.parent;
        }
        _inspectPoint = GameObject.FindWithTag("InspectPoints").GetComponent<Transform>();
    }

    #region Puzzle
//===================================================================================================================================================================================================
    public void GetValue(int value, int index)
    {
        if (index < 0 || index >= _values.Count)
            return;

        _values[index] = value;

        CheckCode();
    }

    private void CheckCode()
    {
        if (PuzzleSequenceManager.Instance.CurrentPuzzle != this)
            return;

        for (int i = 0; i < _codes.Count; i++)
        {
            if (_values[i] != _codes[i])
                return;
        }

        Solve();
    }

    public override void Solve()
    {
        base.Solve();
        
        EventBus.OnCloseEyes?.Invoke();

        Debug.Log("Lock Opened");
    }
//===================================================================================================================================================================================================
    #endregion

    #region XR Interaction
//===================================================================================================================================================================================================
    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_isInspecting) return;
        _isInspecting = true;
        
        _rb.isKinematic = true;
        _rb.useGravity = false;
        
        transform.SetParent(_inspectPoint);
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _grabInteractable.trackPosition = false;
        _grabInteractable.trackRotation = false;
        _inspectPoint.localScale = Vector3.one * _inspectScaleMultiplier;

        EventBus.OnGrabLock?.Invoke();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (!_isInspecting)
            return;

        _isInspecting = false;

        transform.SetParent(_startParent);
        transform.localScale = _startScale;
        if (_useStartPos)
        {
            transform.position = _startPosition;
            transform.rotation = _startRotation;

        }

        _rb.isKinematic = false;

        _rb.useGravity = true;
        _grabInteractable.trackPosition = true;
        _grabInteractable.trackRotation = true;
        
        _inspectPoint.localScale = Vector3.one;
        EventBus.OnReleaseLock?.Invoke();
    }
//===================================================================================================================================================================================================
    #endregion
}