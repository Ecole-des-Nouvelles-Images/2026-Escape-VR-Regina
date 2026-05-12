using UnityEngine;

public class PinDebug : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger enter from: {other.name}", other.gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision enter from: {collision.gameObject.name}", collision.gameObject);
    }
    
    void OnMouseDown()
    {
        Debug.Log("Mouse down (just in case)");
    }
}