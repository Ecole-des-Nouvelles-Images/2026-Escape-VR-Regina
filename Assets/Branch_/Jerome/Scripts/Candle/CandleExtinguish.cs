using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CandleExtinguish : MonoBehaviour
{
    [SerializeField] private Material _flameMaterial;
    [SerializeField] private string _flameActiveProperty = "_FlameActive";
    [SerializeField] private ParticleSystem _extinguish;
    
    private bool _isExtinguished = false;
    private Collider _candleCollider;
    private Material _flameMaterialInstance;
    private int _flamePropertyId;
    private CandleManager _candleManager;

    private void Start()
    {
        // Get the collider component
        _candleCollider = GetComponent<Collider>();
        
        // Find the CandleManager in the scene
        _candleManager = FindAnyObjectByType<CandleManager>();
        
        // Create instance of the flame material to avoid modifying the prefab
        if (_flameMaterial)
        {
            _flameMaterialInstance = new Material(_flameMaterial);
            _flamePropertyId = Shader.PropertyToID(_flameActiveProperty);
            
            // Apply the material instance to the renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer) renderer.material = _flameMaterialInstance;
        }
        else
        {
            Debug.LogError("Flame material not assigned to CandleExtinguish!");
        }
        
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
        // Turn off the flame via shader property (0 = off, 1 = on)
        if (_flameMaterialInstance)
        {
            _flameMaterialInstance.SetFloat(_flamePropertyId, 0f);
        }
        
        // Set the boolean to true
        _isExtinguished = true;
        
        // Turn off the collider
        if (_candleCollider) _candleCollider.enabled = false;
        
        // Play the one-shot extinguish VFX
        if (_extinguish)
        {
            _extinguish.Play();
            Destroy(_extinguish.gameObject, _extinguish.main.duration);
        }
        
        // Notify the manager that this candle has been extinguished
        if (_candleManager) _candleManager.CandleExtinguished(this);
        
        Debug.Log($"Candle {gameObject.name} extinguished!");
    }
    
    public bool GetIsExtinguished() => _isExtinguished;
    
    private void OnDestroy()
    {
        // Clean up the material instance to prevent memory leaks
        if (_flameMaterialInstance)
        {
            Destroy(_flameMaterialInstance);
        }
    }
}