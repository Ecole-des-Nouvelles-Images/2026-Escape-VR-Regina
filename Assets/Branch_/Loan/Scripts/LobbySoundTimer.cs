using System;
using UnityEngine;

public class LobbySoundTimer : MonoBehaviour
{
  [SerializeField] private AudioSource _audioSource;
  [SerializeField] private AudioClip _sound;
  [SerializeField] private float _timeInterval;
  [SerializeField]private float _currentTime;


  private void Start()
  {
    _audioSource.clip = _sound;
    _audioSource.PlayDelayed(2f);
  }

  private void Update()
  {
    _currentTime += Time.deltaTime;

    if (_currentTime >= _timeInterval)
    {
      _audioSource.PlayOneShot(_sound);
      _currentTime = 0;
    }
  }
}
