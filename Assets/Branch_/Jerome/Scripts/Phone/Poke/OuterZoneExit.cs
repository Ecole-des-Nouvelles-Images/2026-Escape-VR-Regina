using UnityEngine;

public class OuterZoneExit : MonoBehaviour
{
    public System.Action<Transform> OnFingerLeftZone;

    private void OnTriggerExit(Collider other)
    {
        // You can filter by layer/tag here if needed
        OnFingerLeftZone?.Invoke(other.transform);
    }
}