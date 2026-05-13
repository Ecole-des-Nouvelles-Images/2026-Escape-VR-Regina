using UnityEngine;

public class TestWheel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TOUCH: " + other.name);
    }
}