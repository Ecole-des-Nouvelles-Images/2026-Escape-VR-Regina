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
    private Quaternion _initialRotation;      // Store the dial's starting rotation
    private float _currentRotationDelta = 0f; // How much rotation has been added (in degrees)
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
    
    private void Start()
    {
        // Store the initial rotation of the dial
        if (_dialToRotate != null)
        {
            _initialRotation = _dialToRotate.rotation;
        }
        else
        {
            Debug.LogError("Dial to rotate reference is missing!", this);
        }
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
            // Increase the rotation delta while being poked
            _currentRotationDelta += _rotationSpeed * Time.deltaTime;
            
            // Clamp to max rotation
            if (_currentRotationDelta >= _maxRotation)
            {
                _currentRotationDelta = _maxRotation;
                StopDialing();
            }
            
            ApplyRotation();
        }
        else if (_isReturning)
        {
            // Decrease the rotation delta back to zero
            _currentRotationDelta -= _returnSpeed * Time.deltaTime;
            
            // Clamp to zero
            if (_currentRotationDelta <= 0)
            {
                _currentRotationDelta = 0;
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
    
    private void StopDialing()
    {
        if (!_isDialing) return;
        
        _isDialing = false;
        _returnTimer = _returnDelay;
        
        // Notify the input handler that dialing has stopped
        if (_inputHandler != null)
            _inputHandler.ReleaseDial();
    }
    
    private void ApplyRotation()
    {
        if (_dialToRotate == null) return;
        
        // Create the delta rotation from the current accumulated angle
        // Negative sign to rotate clockwise (adjust sign as needed for your setup)
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, _currentRotationDelta);
        
        // Combine the initial rotation with the delta rotation
        // Using multiplication applies the delta rotation on top of the initial rotation
        _dialToRotate.rotation = _initialRotation * deltaRotation;
    }
    
    // Public methods for external use
    public bool IsDialing() => _isDialing;
    public bool IsReturning() => _isReturning;
    public float GetCurrentRotation() => _currentRotationDelta;
    
    // Optional: Reset the dial to its initial position
    public void ResetDial()
    {
        _currentRotationDelta = 0;
        _isDialing = false;
        _isReturning = false;
        _returnTimer = 0;
        ApplyRotation();
    }
    
    // Optional: Manually set the rotation delta (for external control)
    public void SetRotationDelta(float deltaDegrees)
    {
        _currentRotationDelta = Mathf.Clamp(deltaDegrees, 0f, _maxRotation);
        ApplyRotation();
    }
}