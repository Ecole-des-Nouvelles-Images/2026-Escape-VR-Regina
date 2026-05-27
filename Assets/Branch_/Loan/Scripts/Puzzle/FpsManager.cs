using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Meta;

public class FpsManager : MonoBehaviour
{
    [Header("===== FPS Settings =====")]
    [SerializeField]  private float _fps;
    [SerializeField] private bool _isFpsLock;
    public static FpsManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        if (_isFpsLock) 
            StartCoroutine(SetRefreshRateCoroutine());
    }
    
    private IEnumerator SetRefreshRateCoroutine()
    {
        yield return null;
            
        List<XRDisplaySubsystem> displays = new();
        SubsystemManager.GetSubsystems(displays);
            
        foreach (var display in displays)
        {
            if (display == null || !display.running) continue;
                
            bool success = display.TryRequestDisplayRefreshRate(_fps);
            if (success) Time.fixedDeltaTime = 1f / _fps;
        }
    }
}