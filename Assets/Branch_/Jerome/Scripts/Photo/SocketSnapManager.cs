using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SocketSnapHandler : MonoBehaviour
{
    [SerializeField] private GameObject _expectedPiece;
    [SerializeField] private ParticleSystem _puzzleCompleted;
    [SerializeField] private Transform _snapPoint;

    private AssemblyManager _manager;
    private Collider _socketCollider;
    private bool _hasSnappedCorrectly = false;

    private void Start()
    {
        _manager = FindFirstObjectByType<AssemblyManager>();
        _socketCollider = GetComponent<Collider>();

        // Safety check: socket collider must be a trigger
        if (_socketCollider != null && !_socketCollider.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Socket collider should be set to 'Is Trigger'.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react to objects tagged "photograph"
        if (!other.CompareTag("photograph")) return;

        // Already snapped - ignore everything
        if (_hasSnappedCorrectly) return;

        GameObject incomingPiece = other.gameObject;

        if (incomingPiece == _expectedPiece)
        {
            AcceptCorrectPiece(incomingPiece);
        }
        else
        {
            RejectWrongPiece(incomingPiece);
        }
    }

    private void AcceptCorrectPiece(GameObject piece)
    {
        _hasSnappedCorrectly = true;

        // Snap to this socket's position and rotation
        piece.transform.position = _snapPoint.transform.position;
        piece.transform.rotation = _snapPoint.transform.rotation;

        // Disable grab interaction
        XRGrabInteractable grabInteractable = piece.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
            grabInteractable.enabled = false;

        // Make kinematic so physics doesn't interfere
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Disable this socket's collider so nothing else can trigger it
        if (_socketCollider != null)
            _socketCollider.enabled = false;

        // Play completion effect
        if (_puzzleCompleted != null)
            _puzzleCompleted.Play();

        // Notify the assembly manager
        if (_manager != null)
            _manager.PiecePlaced(piece, this);
        else
            Debug.LogError("AssemblyManager not found in scene!");

        Debug.Log($"Piece {piece.name} successfully snapped to {gameObject.name}");
    }

    private void RejectWrongPiece(GameObject piece)
    {
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(-transform.forward * 2f, ForceMode.Impulse);

        Debug.Log($"Wrong piece {piece.name} rejected by {gameObject.name}");
    }

    // Call this if you need to reset the slot for testing or replay
    public void ResetSocket()
    {
        _hasSnappedCorrectly = false;

        if (_socketCollider != null)
            _socketCollider.enabled = true;
    }
}