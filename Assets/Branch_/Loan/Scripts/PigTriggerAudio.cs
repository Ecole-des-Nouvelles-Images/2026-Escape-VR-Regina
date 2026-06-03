using System;
using System.Collections.Generic;
using UnityEngine;

public class PigTriggerAudio : MonoBehaviour
{
   [SerializeField] private List<AudioClip> _pigSound;

   private AudioSource _audio;

   private void Start()
   {
      _audio = GetComponent<AudioSource>();
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("FingerTip"))
      {
         if (_pigSound == null || _pigSound.Count == 0) return;
         
         int randomIndex = UnityEngine.Random.Range(0, _pigSound.Count);
         AudioClip clipChoisi = _pigSound[randomIndex];
         
         if (_audio.isPlaying)
         {
            _audio.Stop();
         }
         
         _audio.PlayOneShot(clipChoisi);
      }
   }
}
