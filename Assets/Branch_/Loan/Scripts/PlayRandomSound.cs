using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
    [Header("Sons")]
    [SerializeField] private List<AudioClip> _audioClips;
    private AudioSource _audioSource;

    [Header("Réglages Aléatoires (en secondes)")]
    [SerializeField] private float tempsMinEntreSons = 5f;  // Temps minimum d'attente
    [SerializeField] private float tempsMaxEntreSons = 20f; // Temps maximum d'attente

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        
        if (_audioClips != null && _audioClips.Count > 0)
        {
            StartCoroutine(BoucleSonsAleatoires());
        }
        
    }

    private IEnumerator BoucleSonsAleatoires()
    {
        
        while (true)
        {
            
            float tempsAttente = Random.Range(tempsMinEntreSons, tempsMaxEntreSons);
            yield return new WaitForSeconds(tempsAttente);
            
            yield return new WaitWhile(() => _audioSource.isPlaying);
            
            int indexAleatoire = Random.Range(0, _audioClips.Count);
            AudioClip clipAJouer = _audioClips[indexAleatoire];

            if (clipAJouer != null)
            {
                _audioSource.PlayOneShot(clipAJouer);
                
                Debug.Log($"Son joué : {clipAJouer.name}. Prochain son dans {tempsAttente:F1}s");
            }
        }
    }
}