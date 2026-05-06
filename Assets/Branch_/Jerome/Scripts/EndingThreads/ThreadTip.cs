using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem; // Added for input handling

public class ThreadTip : MonoBehaviour
{
    [Header("References")]
    public ThreadManager threadManager;
    public XRGrabInteractable grabInteractable;
    public float attachDistance = 0.1f;
    public LayerMask pinLayer;
    
    [Header("Input")]
    public InputActionProperty triggerAction; // Assign RightHand Controller / Trigger
    
    [Header("Visual")]
    public GameObject tipVisual;
    public Material validMaterial;
    public Material invalidMaterial;
    public ParticleSystem attachParticles;
    
    [Header("Snap Settings")]
    public float snapDuration = 0.2f;
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isDragging = false;
    private bool isTriggerPressed = false;
    private Pin currentHighlightedPin;
    private Pin pendingAttachPin; // Pin we're hovering over while trigger is held
    private Renderer tipRenderer;
    private Vector3 originalTipScale;
    private bool isSnapping = false;
    
    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        tipRenderer = tipVisual?.GetComponent<Renderer>();
        originalTipScale = transform.localScale;
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }
    
    void OnEnable()
    {
        // Subscribe to trigger input
        if (triggerAction != null)
            triggerAction.action.performed += OnTriggerPressed;
            triggerAction.action.canceled += OnTriggerReleased;
    }
    
    void OnDisable()
    {
        // Unsubscribe from trigger input
        if (triggerAction != null)
        {
            triggerAction.action.performed -= OnTriggerPressed;
            triggerAction.action.canceled -= OnTriggerReleased;
        }
    }
    
    void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!isDragging || isSnapping) return;
        
        isTriggerPressed = true;
        
        // Attempt attachment immediately when trigger is pressed
        if (currentHighlightedPin != null)
        {
            AttemptAttachToPin(currentHighlightedPin);
        }
        else
        {
            StartCoroutine(FlashInvalid());
        }
    }
    
    void OnTriggerReleased(InputAction.CallbackContext context)
    {
        isTriggerPressed = false;
        pendingAttachPin = null;
    }
    
    void Update()
    {
        if (!isDragging || isSnapping) return;
        
        // Find nearest pin within attach distance
        Collider[] nearbyPins = Physics.OverlapSphere(transform.position, attachDistance, pinLayer);
        Pin nearestPin = null;
        float nearestDistance = attachDistance;
        
        foreach (Collider col in nearbyPins)
        {
            Pin pin = col.GetComponent<Pin>();
            if (pin != null && !pin.IsInChain())
            {
                // Use connection point for distance check if available
                Vector3 targetPos = pin.connectionPoint != null ? pin.connectionPoint.position : pin.transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPin = pin;
                }
            }
        }
        
        // Update visual feedback
        if (nearestPin != currentHighlightedPin)
        {
            currentHighlightedPin = nearestPin;
            UpdateTipVisual();
        }
        
        // Scale tip slightly when near a valid pin
        if (currentHighlightedPin != null)
        {
            float proximity = 1f - (nearestDistance / attachDistance);
            float scale = 1f + (proximity * 0.3f);
            transform.localScale = originalTipScale * scale;
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalTipScale, Time.deltaTime * 10f);
        }
    }
    
    void AttemptAttachToPin(Pin targetPin)
    {
        if (targetPin == null) return;
        if (threadManager == null) return;
        
        bool success = threadManager.AddPin(targetPin);
        
        if (success)
        {
            if (attachParticles != null)
                attachParticles.Play();
            
            // Snap to pin position
            StartCoroutine(SnapToPin(targetPin));
        }
        else
        {
            StartCoroutine(FlashInvalid());
        }
    }
    
    void OnGrab(SelectEnterEventArgs args)
    {
        isDragging = true;
        if (tipVisual != null)
            tipVisual.SetActive(true);
    }
    
    void OnRelease(SelectExitEventArgs args)
    {
        isDragging = false;
        isTriggerPressed = false;
        currentHighlightedPin = null;
        pendingAttachPin = null;
        
        if (tipVisual != null)
            tipVisual.SetActive(false);
        
        UpdateTipVisual();
        transform.localScale = originalTipScale;
    }
    
    IEnumerator SnapToPin(Pin targetPin)
    {
        isSnapping = true;
        
        // Disable grab and input during snap
        if (grabInteractable != null)
            grabInteractable.enabled = false;
        
        Vector3 targetPos = targetPin.connectionPoint != null ? 
            targetPin.connectionPoint.position : targetPin.transform.position;
        
        // Add small offset so tip sits next to pin, not inside it
        Vector3 direction = (transform.position - targetPos).normalized;
        targetPos += direction * 0.03f;
        
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(targetPin.transform.forward);
        
        float elapsed = 0f;
        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = snapCurve.Evaluate(elapsed / snapDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        
        transform.position = targetPos;
        transform.rotation = targetRot;
        
        // Small delay before re-enabling
        yield return new WaitForSeconds(0.1f);
        
        if (grabInteractable != null)
            grabInteractable.enabled = true;
        
        isSnapping = false;
    }
    
    IEnumerator FlashInvalid()
    {
        if (tipRenderer != null && invalidMaterial != null)
        {
            Material originalMat = tipRenderer.material;
            tipRenderer.material = invalidMaterial;
            yield return new WaitForSeconds(0.15f);
            tipRenderer.material = originalMat;
        }
    }
    
    void UpdateTipVisual()
    {
        if (tipRenderer != null)
        {
            if (currentHighlightedPin != null && validMaterial != null)
                tipRenderer.material = validMaterial;
            else if (invalidMaterial != null)
                tipRenderer.material = invalidMaterial;
        }
    }
    
    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}