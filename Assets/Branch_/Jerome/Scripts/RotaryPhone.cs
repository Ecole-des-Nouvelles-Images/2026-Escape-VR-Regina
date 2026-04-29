using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRGrabRotaryDial : MonoBehaviour
{
    public float maxRotationAngle = 330f;
    public float springForce = 10f;
    
    private Transform parentPivot;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private float currentAngle = 0f;
    private Quaternion startLocalRot;
    private Vector3 startLocalPos;
    
    void Start()
    {
        parentPivot = transform.parent;
        startLocalRot = transform.localRotation;
        startLocalPos = transform.localPosition;
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                            RigidbodyConstraints.FreezeRotationZ;
        }
        
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }
    
    void FixedUpdate()
    {
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // While grabbed, update angle based on position
            Vector3 toInteractor = grabInteractable.interactorsSelecting[0].transform.position - parentPivot.position;
            toInteractor.y = 0;
            
            float targetAngle = Vector3.SignedAngle(Vector3.forward, toInteractor, Vector3.up);
            targetAngle = Mathf.Clamp(targetAngle, 0f, maxRotationAngle);
            
            currentAngle = targetAngle;
            ApplyRotation(currentAngle);
        }
        else if (currentAngle > 0f)
        {
            // Spring back when released
            currentAngle = Mathf.Lerp(currentAngle, 0f, Time.fixedDeltaTime * springForce);
            
            if (currentAngle < 0.5f)
            {
                currentAngle = 0f;
                ResetTransform();
            }
            else
            {
                ApplyRotation(currentAngle);
            }
        }
    }
    
    void ApplyRotation(float angle)
    {
        transform.localRotation = startLocalRot;
        transform.localPosition = startLocalPos;
        transform.RotateAround(parentPivot.position, Vector3.up, -angle);
    }
    
    void ResetTransform()
    {
        transform.localRotation = startLocalRot;
        transform.localPosition = startLocalPos;
    }
    
    void OnRelease(SelectExitEventArgs args)
    {
        // Trigger dial number on release
        int number = Mathf.FloorToInt((currentAngle / maxRotationAngle) * 10f);
        if (number == 0) number = 10;
        Debug.Log($"Dialed: {number}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Number"))
        {
            Debug.Log(other.name);
        }
    }
}