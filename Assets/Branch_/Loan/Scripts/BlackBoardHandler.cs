using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BlackBoardHandler : MonoBehaviour
{
    [Header("===== Settings =====")] 
    [SerializeField]
    private GameObject _expectedPrefab;
    
    [SerializeField]
    private Transform _attachmentTransform;
    
    [Header("===== Debug =====")]
    public bool IsOccuped;
    public GameObject CurrentObject { get; private set; }

    private BlackBoardManager _blackBoardManager;
    private Collider _triggerCollider;
    private Rigidbody _currentObjectRb;

    private void Start()
    {
        _blackBoardManager = GetComponentInParent<BlackBoardManager>();
        _triggerCollider = GetComponent<Collider>();
        
        if (_triggerCollider == null)
        {
            Debug.LogError($"No Collider found on {gameObject.name}. Please add a trigger collider.");
        }
        else
        {
            _triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the "photograph" tag
        if (!other.CompareTag("photograph"))
            return;

        // If socket is already occupied, don't allow another object
        if (IsOccuped)
            return;

        GameObject enteredObject = other.gameObject;

        // Check if it's the expected object
        if (enteredObject != _expectedPrefab)
        {
            // Wrong object placed - game lose
            EventBus.OnGameLoose?.Invoke();
            return;
        }

        // Correct object placed
        CurrentObject = enteredObject;
        IsOccuped = true;

        // Snap the object to the attachment transform
        SnapObjectToAttachment(enteredObject);

        // Notify the manager
        _blackBoardManager.SocketIsOccuped();
    }

    private void SnapObjectToAttachment(GameObject obj)
    {
        XRGrabInteractable _currentObjectGrab = obj.GetComponent<XRGrabInteractable>();
        _currentObjectGrab.enabled = false;
        
        // Store the Rigidbody for physics adjustments
        _currentObjectRb = obj.GetComponent<Rigidbody>();
        
        if (_currentObjectRb != null)
        {
            // Disable physics while snapped
            _currentObjectRb.isKinematic = true;
            _currentObjectRb.useGravity = false;
        }

        // Snap to attachment position and rotation
        obj.transform.position = _attachmentTransform.position;
        obj.transform.rotation = _attachmentTransform.rotation;
        
        // Optional: Parent the object to the attachment point
        obj.transform.SetParent(_attachmentTransform);
        
        // Optional: Disable the collider to prevent further trigger interactions
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }
    }

    public bool IsObject()
    {
        if (!IsOccuped)
            return false;

        // Check if the current object matches the expected prefab
        return CurrentObject == _expectedPrefab;
    }

    // Optional: Method to manually clear the socket (for resetting puzzles)
    public void ClearSocket()
    {
        if (CurrentObject != null)
        {
            Destroy(CurrentObject);
            CurrentObject = null;
        }
        
        IsOccuped = false;
        _currentObjectRb = null;
    }
}