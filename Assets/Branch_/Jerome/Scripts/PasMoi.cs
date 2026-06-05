using System.Collections;
using UnityEngine;

public class PasMoi : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    
    [Header("Timing Settings")]
    [SerializeField] private float minDelay = 2f;
    [SerializeField] private float maxDelay = 5f;
    [SerializeField] private bool startOnEnable = true;
    
    private Coroutine audioCoroutine;
    
    private void Start()
    {
        // Get AudioSource if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // Create AudioSource if it doesn't exist
        if (audioSource == null && audioClip != null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Set the clip if provided
        if (audioClip != null && audioSource != null)
            audioSource.clip = audioClip;
    }
    
    private void OnEnable()
    {
        if (startOnEnable)
            StartRandomAudio();
    }
    
    private void OnDisable()
    {
        StopRandomAudio();
    }
    
    public void StartRandomAudio()
    {
        if (audioCoroutine != null)
            StopCoroutine(audioCoroutine);
        
        audioCoroutine = StartCoroutine(PlayAudioRandomly());
    }
    
    public void StopRandomAudio()
    {
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
            audioCoroutine = null;
        }
        
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
    
    private IEnumerator PlayAudioRandomly()
    {
        while (true)
        {
            // Get random delay
            float randomDelay = Random.Range(minDelay, maxDelay);
            
            // Wait for the random duration
            yield return new WaitForSeconds(randomDelay);
            
            // Play the audio
            if (audioSource != null && audioClip != null)
            {
                audioSource.Play();
                Debug.Log($"Playing audio after {randomDelay:F2} seconds");
            }
            else
            {
                Debug.LogWarning("AudioSource or AudioClip is missing!");
                yield break;
            }
        }
    }
    
    // Optional: Manually trigger a play with a new random delay
    public void PlayWithNewRandomDelay()
    {
        if (audioCoroutine != null)
        {
            StopRandomAudio();
            StartRandomAudio();
        }
    }
}
