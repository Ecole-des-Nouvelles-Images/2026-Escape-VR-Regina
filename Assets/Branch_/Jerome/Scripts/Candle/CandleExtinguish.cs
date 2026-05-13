using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class CandleExtinguish : MonoBehaviour
{
    private bool _isExtinguished = false;
    private Collider _candleCollider;
    private Renderer _candleRenderer;

    void Start()
    {
        // Get the collider component on this game object
        _candleCollider = GetComponent<Collider>();
        
        // Get the renderer component to change color
        _candleRenderer = GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the tag "CandleSnuffer" and the candle isn't already extinguished
        if (other.CompareTag("CandleSnuffer") && !_isExtinguished)
        {
            ExtinguishCandle();
        }
    }

    void ExtinguishCandle()
    {
        // Change color to green
        if (_candleRenderer != null)
        {
            _candleRenderer.material.color = Color.green;
        }
        
        // Set the boolean to true
        _isExtinguished = true;
        
        // Turn off the collider
        if (_candleCollider != null)
        {
            _candleCollider.enabled = false;
        }
        
        Debug.Log("Candle extinguished!");
    }
    
    public bool GetIsExtinguished() => _isExtinguished;
}