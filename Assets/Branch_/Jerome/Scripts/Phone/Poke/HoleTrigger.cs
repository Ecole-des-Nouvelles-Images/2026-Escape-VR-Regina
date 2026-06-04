using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class HoleTrigger : MonoBehaviour
{
    [Header("Hole Settings")]
    [SerializeField] private float _maxRotationDegrees = 30f; // e.g. hole 1 = 30°, hole 3 = 90°, etc.

    public float MaxRotationDegrees => _maxRotationDegrees;

    // Events for the manager
    public System.Action<HoleTrigger, Transform> OnFingerEnter;
    public System.Action<HoleTrigger, Transform> OnFingerExit;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"HoleTrigger on {name}: collider is not a trigger. Enabling trigger.", this);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter me");
        // Assumes the fingertip has a specific tag or layer. You can replace with layer check.
        // We'll assume the manager uses a layer mask, but we still report all.
        // For performance, you can check tag: if (!other.CompareTag("Fingertip")) return;
        OnFingerEnter?.Invoke(this, other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        OnFingerExit?.Invoke(this, other.transform);
    }
}