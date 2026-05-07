using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ThreadTip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThreadManager _threadManager;
    [SerializeField] private XRGrabInteractable _grabInteractable;
    [SerializeField] private float _attachDistance = 0.1f;
    [SerializeField] private LayerMask _pinLayer;
    
    [Header("Visual")]
    [SerializeField] private GameObject _tipVisual;
    [SerializeField] private Material _validMaterial;
    [SerializeField] private Material _invalidMaterial;
    [SerializeField] private ParticleSystem _attachParticles;
    
    private bool _isDragging = false;
    private Pin _currentHighlightedPin;
    private Renderer _tipRenderer;
    private Vector3 _originalTipScale;
    private bool _isAttaching = false;

    private void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _tipRenderer = _tipVisual?.GetComponent<Renderer>();
        _originalTipScale = transform.localScale;
        
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(OnGrab);
            _grabInteractable.selectExited.AddListener(OnRelease);
            _grabInteractable.activated.AddListener(OnTriggerPressed);
        }
    }

    private void Update()
    {
        if (!_isDragging || _isAttaching) return;
        
        // Find nearest pin within attach distance
        Collider[] nearbyPins = Physics.OverlapSphere(transform.position, _attachDistance, _pinLayer);
        Pin nearestPin = null;
        float nearestDistance = _attachDistance;
        
        foreach (Collider col in nearbyPins)
        {
            Pin pin = col.GetComponent<Pin>();
            if (pin != null && !pin.IsInChain())
            {
                // Use connection point for distance check if available
                Vector3 targetPos = pin.ConnectionPoint != null ? pin.ConnectionPoint.position : pin.transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPin = pin;
                }
            }
        }
        
        // Update visual feedback
        if (nearestPin != _currentHighlightedPin)
        {
            _currentHighlightedPin = nearestPin;
            UpdateTipVisual();
        }
        
        // Scale tip slightly when near a valid pin
        if (_currentHighlightedPin != null)
        {
            float proximity = 1f - (nearestDistance / _attachDistance);
            float scale = 1f + (proximity * 0.3f);
            transform.localScale = _originalTipScale * scale;
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _originalTipScale, Time.deltaTime * 10f);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _isDragging = true;
        if (_tipVisual != null)
            _tipVisual.SetActive(true);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        _isDragging = false;
        _isAttaching = false;
        
        if (_tipVisual != null)
            _tipVisual.SetActive(false);
        
        _currentHighlightedPin = null;
        UpdateTipVisual();
        transform.localScale = _originalTipScale;
    }

    private void OnTriggerPressed(ActivateEventArgs args)
    {
        if (!_isDragging || _isAttaching) return;
        
        // Try to attach to nearest pin when trigger is pressed
        Collider[] nearbyPins = Physics.OverlapSphere(transform.position, _attachDistance, _pinLayer);
        Pin nearestPin = null;
        float nearestDistance = _attachDistance;
        
        foreach (Collider col in nearbyPins)
        {
            Pin pin = col.GetComponent<Pin>();
            if (pin != null && !pin.IsInChain())
            {
                Vector3 targetPos = pin.ConnectionPoint != null ? pin.ConnectionPoint.position : pin.transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPin = pin;
                }
            }
        }
        
        if (nearestPin != null)
        {
            _isAttaching = true;
            
            // Success - attach to pin
            if (_threadManager != null)
            {
                bool success = _threadManager.AddPin(nearestPin);
                if (success)
                {
                    if (_attachParticles != null)
                        _attachParticles.Play();
                    
                    // Visual feedback only - no snap, no release
                    StartCoroutine(AttachFeedback());
                }
                else
                {
                    StartCoroutine(FlashInvalid());
                    _isAttaching = false;
                }
            }
        }
        else
        {
            // Flash red to show invalid
            StartCoroutine(FlashInvalid());
        }
    }

    private IEnumerator AttachFeedback()
    {
        // Quick visual pulse to confirm attachment
        if (_tipRenderer != null && _validMaterial != null)
        {
            Material originalMat = _tipRenderer.material;
            _tipRenderer.material = _validMaterial;
            yield return new WaitForSeconds(0.1f);
            _tipRenderer.material = originalMat;
        }
        
        _isAttaching = false;
    }

    private IEnumerator FlashInvalid()
    {
        if (_tipRenderer != null && _invalidMaterial != null)
        {
            Material originalMat = _tipRenderer.material;
            _tipRenderer.material = _invalidMaterial;
            yield return new WaitForSeconds(0.15f);
            _tipRenderer.material = originalMat;
        }
    }

    private void UpdateTipVisual()
    {
        if (_tipRenderer != null)
        {
            if (_currentHighlightedPin != null && _validMaterial != null)
                _tipRenderer.material = _validMaterial;
            else if (_invalidMaterial != null)
                _tipRenderer.material = _invalidMaterial;
        }
    }

    private void OnDestroy()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrab);
            _grabInteractable.selectExited.RemoveListener(OnRelease);
            _grabInteractable.activated.RemoveListener(OnTriggerPressed);
        }
    }
}