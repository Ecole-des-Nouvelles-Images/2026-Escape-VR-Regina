using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class CandleExtinguish : MonoBehaviour
{
    private bool _isExtinguished = false;
    private Collider _candleCollider;
    private Renderer _candleRenderer;
    private CandleManager _candleManager;

    private void Start()
    {
        // Get the collider component on this game object
        _candleCollider = GetComponent<Collider>();
        
        // Get the renderer component to change color
        _candleRenderer = GetComponent<Renderer>();
        
        // Find the CandleManager in the scene
        _candleManager = FindAnyObjectByType<CandleManager>();
        
        // Register this candle with the manager
        if (_candleManager)
        {
            _candleManager.RegisterCandle(this);
        }
        else
        {
            Debug.LogWarning("No CandleManager found in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the tag "CandleSnuffer" and the candle isn't already extinguished
        if (other.CompareTag("CandleSnuffer") && !_isExtinguished)
        {
            ExtinguishCandle();
        }
    }

    private void ExtinguishCandle()
    {
        // Change color to green
        if (_candleRenderer) _candleRenderer.material.color = Color.green;
        
        // Set the boolean to true
        _isExtinguished = true;
        
        // Turn off the collider
        if (_candleCollider) _candleCollider.enabled = false;
        
        // Notify the manager that this candle has been extinguished
        if (_candleManager) _candleManager.CandleExtinguished(this);
        
        Debug.Log($"Candle {gameObject.name} extinguished!");
    }
    
    public bool GetIsExtinguished() => _isExtinguished;
}