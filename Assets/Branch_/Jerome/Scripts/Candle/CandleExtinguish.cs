using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class CandleExtinguish : MonoBehaviour
{
    [SerializeField] private string _flameActiveProperty;
    [SerializeField] private ParticleSystem _extinguish;
    
    private bool _isExtinguished = false;
    private Collider _candleCollider;
    private Renderer _candleRenderer;
    private Material _flameMaterial;
    private int _flamePropertyId;
    private CandleManager _candleManager;

    private void Start()
    {
        // Get components
        _candleCollider = GetComponent<Collider>();
        _candleRenderer = GetComponent<Renderer>();
        
        // Get the existing material (automatically becomes an instance when modified)
        if (_candleRenderer)
        {
            _flameMaterial = _candleRenderer.material; // This creates a unique instance automatically
            _flamePropertyId = Shader.PropertyToID(_flameActiveProperty);
        }
        else
        {
            Debug.LogError("Renderer not found on candle!");
        }
        
        // Find the CandleManager
        _candleManager = FindAnyObjectByType<CandleManager>();
        
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
        if (other.CompareTag("CandleSnuffer") && !_isExtinguished)
        {
            ExtinguishCandle();
        }
    }

    private void ExtinguishCandle()
    {
        // Turn off the flame via shader property
        if (_flameMaterial)
        {
            _flameMaterial.SetFloat(_flamePropertyId, 0f);
        }
        
        _isExtinguished = true;
        
        if (_candleCollider) _candleCollider.enabled = false;
        
        if (_extinguish)
        {
            _extinguish.Play();
            Destroy(_extinguish.gameObject, _extinguish.main.duration);
        }
        
        if (_candleManager) _candleManager.CandleExtinguished(this);
        
        Debug.Log($"Candle {gameObject.name} extinguished!");
    }
    
    public bool GetIsExtinguished() => _isExtinguished;
}