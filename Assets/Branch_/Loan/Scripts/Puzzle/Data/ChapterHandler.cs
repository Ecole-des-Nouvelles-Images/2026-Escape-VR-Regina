using System.Collections;
using UnityEngine;
public class ChapterHandler : MonoBehaviour
{
    [SerializeField] private ChapterData _sceneChapter;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    private void Start()
    {
        if (PuzzleSequenceManager.Instance != null && _sceneChapter != null)
        {
            PuzzleSequenceManager.Instance.InjectCurrentChapter(_sceneChapter);
        }else
        {
            Debug.LogWarning("Manager global introuvable ou ScriptableObject manquant !");
        }

        if (_audioSource != null)
        {
            _audioSource.clip = _audioClip;
            _audioSource.PlayDelayed(1.5f);
        }
        else
        {
            _audioSource = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
            StartCoroutine(AttendreEtJouer());
        }
    }
    
    private IEnumerator AttendreEtJouer()
    {
        // "Tant que l'AudioSource est en train de jouer, attends la frame suivante"
        yield return new WaitWhile(() => _audioSource.isPlaying);

        // Une fois que c'est fini, on prend la main
        _audioSource.clip = _audioClip;
        _audioSource.Play();
    }
}