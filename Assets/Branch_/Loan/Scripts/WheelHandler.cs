using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WheelHandler : MonoBehaviour
{
    [SerializeField] private int _index;
    public int Value; 
    [SerializeField] private GameObject _visual; 
    private XRGrabInteractable grab;
    private Rigidbody rb;
    [SerializeField] private float _angle;
    [SerializeField] private LockManager _lockManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>(); 
        
        grab.selectExited.AddListener(OnSelectExited);
    }

    private void Update()
    {
        _angle = transform.eulerAngles.y; 
        
        Value = Mathf.RoundToInt(_angle / 36f) % 10; 
    
        // On s'assure que la valeur reste positive (0-9)
        if (Value < 0) Value += 10;
        float snappedAngle = Value * 36f; 
        
        _visual.transform.localRotation = Quaternion.Euler(0, snappedAngle, 0);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        
        float finalAngle = Value * 36f; 
        transform.localRotation = Quaternion.Euler(0, finalAngle, 0);
        _lockManager.GetValue(Value,_index);
    }
}
