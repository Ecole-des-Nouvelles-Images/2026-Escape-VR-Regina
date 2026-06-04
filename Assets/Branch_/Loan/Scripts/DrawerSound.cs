using System;
using UnityEngine;
using UnityEngine.Serialization;

public class DrawerSound : MonoBehaviour
{
    [FormerlySerializedAs("drawerSound")]
    [Header("Audio Settings")]
    [SerializeField] private AudioClip _drawerSound; 
    [FormerlySerializedAs("maxVolume")] [SerializeField] private float _maxVolume = 1.0f;
    
    [FormerlySerializedAs("speedThreshold")]
    [Header("Physics Settings")]
    [SerializeField] private float _speedThreshold = 0.05f; // Vitesse minimum pour déclencher le son
    [FormerlySerializedAs("maxSpeedVelocity")] [SerializeField] private float _maxSpeedVelocity = 1.5f; // Vitesse à laquelle le son est au volume max
    [FormerlySerializedAs("volumeFadeSpeed")] [SerializeField] private float _volumeFadeSpeed = 5f; // Douceur de la transition du son

    private Rigidbody _rb;
    private AudioSource _audioSource;
    private float _targetVolume = 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
       
        _audioSource.clip = _drawerSound;
        _audioSource.loop = true; 
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1.0f; 
        _audioSource.volume = 0f;
    }

    void Update()
    {
        
        float currentSpeed = _rb.linearVelocity.magnitude;

        
        if (currentSpeed > _speedThreshold)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            
            float speedRatio = Mathf.InverseLerp(_speedThreshold, _maxSpeedVelocity, currentSpeed);
            
            _targetVolume = speedRatio * _maxVolume;
            _audioSource.pitch = Mathf.Lerp(0.85f, 1.15f, speedRatio); // Variation de pitch organique
        }
        else
        {
            
            _targetVolume = 0f;
        }

       
        _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _targetVolume, _volumeFadeSpeed * Time.deltaTime);
        
        if (_audioSource.volume <= 0f && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
    
    
}
