using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start() {
       
        if (masterSlider == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("AudioSettingsUI: Sliders are not assigned in the inspector.");
            return;
        }
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager instance not found. Please ensure it is initialized before AudioSettingsUI.");
            return;
        }

        masterSlider.value = SoundManager.Instance.masterVolume;
        musicSlider.value = SoundManager.Instance.musicVolume;
        sfxSlider.value = SoundManager.Instance.sfxVolume;

        masterSlider.onValueChanged.AddListener((v) => {
        SoundManager.Instance.masterVolume = v;
        SoundManager.Instance.ApplyVolumeSettings();
    });
         musicSlider.onValueChanged.AddListener((v) => {
        SoundManager.Instance.musicVolume = v;
        SoundManager.Instance.ApplyVolumeSettings();
    });
        sfxSlider.onValueChanged.AddListener((v) => {
        SoundManager.Instance.sfxVolume = v;
        SoundManager.Instance.ApplyVolumeSettings();
    });
    }

    void OnMasterVolumeChanged(float value) {
        SoundManager.Instance.masterVolume = value;
        SoundManager.Instance.ApplyVolumeSettings();
    }

    void OnMusicVolumeChanged(float value) {
        SoundManager.Instance.musicVolume = value;
        SoundManager.Instance.ApplyVolumeSettings();
    }

    void OnSFXVolumeChanged(float value) {
        SoundManager.Instance.sfxVolume = value;
        SoundManager.Instance.ApplyVolumeSettings();
    }

    public void SaveSettings() {
        SoundManager.Instance.SaveAudioSettings();
    }
}

