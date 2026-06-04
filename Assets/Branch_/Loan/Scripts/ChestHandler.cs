using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ChestHandler : MonoBehaviour
{
    [SerializeField ] private GameObject _topChest;
    [SerializeField] private float _duration;
    [SerializeField] private AudioSource _chestAudioSource;
    [SerializeField] private AudioClip _chestSound;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private float _delay;

    private void Start()
    {
       _audioSource = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
    }

    [ContextMenu("Open Chest")]
    public void OpenChest()
    {
        StartCoroutine(OpenChestCoroutine());
    }
    
   private IEnumerator OpenChestCoroutine()
    {
        if (_topChest == null) yield break;

        
        _chestAudioSource.PlayOneShot(_chestSound);
        // On crée notre conteneur d'animations
        Sequence openSequence = DOTween.Sequence();

        // Target de rotation pour le couvercle (sur l'axe Z)
        Vector3 targetRotation = new Vector3(_topChest.transform.localEulerAngles.x, _topChest.transform.localEulerAngles.y, -140f);
        
        // A. Le coffre entier vibre légèrement (effet mécanique/magique)
        Transform shakeTarget = gameObject != null ? gameObject.transform : transform;
        openSequence.Append(shakeTarget.DOShakeRotation(0.4f, new Vector3(2f, 2f, 2f), 30));
        
        // B. En même temps, le coffre se gonfle de 8% sur l'axe Y (effet "pression")
        openSequence.Join(shakeTarget.DOPunchScale(new Vector3(0f, 0.08f, 0f), 0.4f, 5, 1f));

        // On attend que cette phase de tremblement soit finie
        openSequence.AppendInterval(0.1f);
        
        // Le couvercle tourne vers -140° en Z.
        // L'Ease "OutBack" va faire s'ouvrir le coffre un peu trop grand (ex: -145°) 
        // puis le faire revenir se stabiliser à -140° avec un effet de rebond lourd très réaliste.
        openSequence.Append(_topChest.transform.DOLocalRotate(targetRotation, 1.2f, RotateMode.FastBeyond360).SetEase(Ease.OutBack));
        
        _audioSource.clip = _audioClip;
        _audioSource.PlayDelayed(_delay);

        // Magie DOTween : On attend que toute la séquence soit terminée avant de couper la coroutine
        yield return openSequence.WaitForCompletion();
    }
    
}
