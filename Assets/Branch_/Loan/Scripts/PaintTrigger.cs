using System;
using UnityEngine;

public class PaintTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _audioClip;
    private bool _isPlay;
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")|| other.CompareTag("FingerTip") && !_isPlay)
        {
            _audio.PlayOneShot(_audioClip);
        }
    }
}
