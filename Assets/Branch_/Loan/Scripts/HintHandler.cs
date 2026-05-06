using UnityEngine;

public class HintHandler : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    
    void OnTriggerEnter(Collider other)
    {
        if (other == _collider)
        {
            Debug.Log("Entered a hint");
        }
    }
}
