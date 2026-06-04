using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public struct VolumeSliderGroup
    {
        [FormerlySerializedAs("mixerParameterName")] public string MixerParameterName; // Ex: "MasterVolume", "MusicVolume"...
        [FormerlySerializedAs("slider")] public Slider Slider;
    }

    [FormerlySerializedAs("myAudioMixer")]
    [Header("Configuration")]
    [SerializeField] private AudioMixer _myAudioMixer;
    [FormerlySerializedAs("volumeSliders")] [SerializeField] private VolumeSliderGroup[] _volumeSliders;

    private void Start()
    {
        // On initialise chaque slider présent dans la liste
        foreach (var group in _volumeSliders)
        {
            if (group.Slider == null) continue;

            // 1. Récupération de la valeur sauvegardée (0.5f par défaut si première fois)
            float savedVolume = PlayerPrefs.GetFloat(group.MixerParameterName, 0.5f);

            // 2. Application de la valeur au Slider
            group.Slider.value = savedVolume;

            // 3. Application immédiate au Mixer via une coroutine
            StartCoroutine(SetVolumeDelayed(group.MixerParameterName, savedVolume));

            // 4. On écoute les changements en direct
            group.Slider.onValueChanged.AddListener((value) => SetVolume(group.MixerParameterName, value));
        }
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        // Formule linéaire plus stable pour éviter les coupures micro-logarithmiques :
        // Si le slider est à 0 = coupé (-80dB). Sinon, varie de -40dB (très faible) à 0dB (max).
        float dBValue = sliderValue > 0 ? Mathf.Lerp(-40f, 0f, sliderValue) : -80f;

        // On applique au Mixer
        _myAudioMixer.SetFloat(parameterName, dBValue);

        // Sauvegarde de la préférence
        PlayerPrefs.SetFloat(parameterName, sliderValue);
    }

    private IEnumerator SetVolumeDelayed(string parameterName, float sliderValue)
    {
        yield return new WaitForSeconds(0.1f); // Un tout petit délai pour s'assurer que le Mixer est réveillé
        SetVolume(parameterName, sliderValue);
    }
}