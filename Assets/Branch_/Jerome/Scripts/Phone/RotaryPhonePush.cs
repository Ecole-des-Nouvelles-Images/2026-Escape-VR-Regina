using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(XRPokeFilter))]
public class RotaryDialPush : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 200f;     // degrees/sec while pushing
    [SerializeField] private float _returnSpeed = 300f;       // degrees/sec while returning
    [SerializeField] private float _maxRotation = 330f;
    [SerializeField] private float _returnDelay = 0.2f;
    
    [Header("References")]
    [SerializeField] private Transform _dialToRotate;
    [SerializeField] private RotaryPhoneInputHandler _inputHandler;
    
    private XRSimpleInteractable _interactable;
    private float _currentRotation = 0f;
    private bool _isDialing = false;
    private bool _isReturning = false;
    private float _returnTimer = 0f;
    
    private void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        
        if (!_inputHandler) _inputHandler = FindFirstObjectByType<RotaryPhoneInputHandler>();
        // Set up interactable events
        _interactable.selectEntered.AddListener(OnPokeEnter);
        _interactable.selectExited.AddListener(OnPokeExit);
    }
    
    private void OnDestroy()
    {
        if (!_interactable) return;
        
        _interactable.selectEntered.RemoveListener(OnPokeEnter);
        _interactable.selectExited.RemoveListener(OnPokeExit);
    }
    
    private void Update()
    {
        if (_isDialing)
        {
            // Rotate clockwise while being poked
            _currentRotation += _rotationSpeed * Time.deltaTime;
            
            if (_currentRotation >= _maxRotation)
            {
                _currentRotation = _maxRotation;
                StopDialing();
            }
            
            ApplyRotation();
        }
        else if (_isReturning)
        {
            // Rotate back to zero
            _currentRotation -= _returnSpeed * Time.deltaTime;
            
            if (_currentRotation <= 0)
            {
                _currentRotation = 0;
                _isReturning = false;
            }
            
            ApplyRotation();
        }
        else if (_returnTimer > 0)
        {
            // Count down the return delay
            _returnTimer -= Time.deltaTime;
            if (_returnTimer <= 0)
            {
                _isReturning = true;
                _returnTimer = 0;
            }
        }
    }
    private void OnPokeEnter(SelectEnterEventArgs args)
    {
        // Cancel any pending return
        _returnTimer = 0;
        _isReturning = false;
        _isDialing = true;
    }
    
    private void OnPokeExit(SelectExitEventArgs args)
    {
        StopDialing();
    }
    
    public void StopDialing()
    {
        if (!_isDialing) return;
        
        _isDialing = false;
        _returnTimer = _returnDelay;
        _inputHandler.ReleaseDial();
    }
    
    private void ApplyRotation()
    {
        if (_dialToRotate != null)
            _dialToRotate.localRotation = Quaternion.Euler(0f, 0f, _currentRotation);
    }
    
    // Public methods for external use
    public bool IsDialing() => _isDialing;
    public bool IsReturning() => _isReturning;
    public float GetCurrentRotation() => _currentRotation;
}