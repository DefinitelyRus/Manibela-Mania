using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour

{
    public static SoundManager Instance;

    public AudioSource loopSource;   // e.g., engine
    public AudioSource sfxSource;    // one-shot
    public AudioSource musicSourceA;
    public AudioSource musicSourceB;

    public AudioClip[] menuMusicTracks;
    public AudioClip[] gameMusicTracks;

    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;
    private Coroutine fadeCoroutine;

    private List<AudioSource> extraSFXSources = new List<AudioSource>();

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private bool isInMenu = true;



    public void RegisterExtraSFXSource(AudioSource source)
    {
        if (source != null && !extraSFXSources.Contains(source))
        {
            extraSFXSources.Add(source);
            ApplyVolumeSettings();
        }
    }
    public void StopMusicLoop()
    {
        StopAllCoroutines();
        if (currentMusicSource != null)
            currentMusicSource.Stop();
        if (nextMusicSource != null)
            nextMusicSource.Stop();
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Only assign music sources if available
        if (musicSourceA != null && musicSourceB != null)
        {
            currentMusicSource = musicSourceA;
            nextMusicSource = musicSourceB;
        }

        LoadAudioSettings();
    }

    public void SetSceneSFXSources(AudioSource newSFX, AudioSource newLoop)
    {
        sfxSource = newSFX;
        loopSource = newLoop;
        ApplyVolumeSettings();
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Example: only reassign in gameplay scene
        if (scene.name == "MainRoad")
        {
            loopSource = GameObject.Find("LoopSource")?.GetComponent<AudioSource>();
            sfxSource = GameObject.Find("SFXSource")?.GetComponent<AudioSource>();

            // Optional: reassign music sources if they are scene-based (NOT recommended)

            SwitchToGameMusic();
            ApplyVolumeSettings();
        }
    }


    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void StopMusic()
    {
        StopAllCoroutines();

        if (currentMusicSource != null) currentMusicSource.Stop();
        if (nextMusicSource != null) nextMusicSource.Stop();
    }
    public void PlayMenuMusic()
    {
        isInMenu = true;
        StopAllCoroutines();
        StartCoroutine(PlayMusicLoop(menuMusicTracks));
    }

    void Start()
    {
        if (menuMusicTracks != null && menuMusicTracks.Length > 0 && musicSourceA != null && musicSourceB != null)
        {
            PlayMenuMusic(); // Safe to call only if music sources are present
        }
    }
    public void SwitchToGameMusic()
    {
        isInMenu = false;
        StopAllCoroutines();
        StartCoroutine(PlayMusicLoop(gameMusicTracks));
    }

    public void SwitchToMenuMusic()
    {
        isInMenu = true;
        StopAllCoroutines();
        StartCoroutine(PlayMusicLoop(menuMusicTracks));
    }


    private IEnumerator PlayMusicLoop(AudioClip[] tracks)
    {
        if (tracks == null || tracks.Length == 0 || currentMusicSource == null || nextMusicSource == null)
            yield break;

        while (true)
        {
            AudioClip nextClip;

            // If only one track, no need to check equality to prevent infinite loop (culprit for infinite loop ffs)
            if (tracks.Length == 1)
            {
                nextClip = tracks[0];
            }
            else
            {
                do
                {
                    nextClip = tracks[Random.Range(0, tracks.Length)];
                }
                while (nextClip == currentMusicSource.clip);
            }

            yield return StartCoroutine(CrossfadeTo(nextClip, 2f));

            // Safely wait for the current music to finish playing
            yield return new WaitUntil(() => currentMusicSource != null && !currentMusicSource.isPlaying);
        }
    }




    private IEnumerator CrossfadeTo(AudioClip newClip, float duration)
    {
        if (nextMusicSource == null || currentMusicSource == null)
            yield break;

        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f;
        nextMusicSource.Play();

        float time = 0f;
        float startVolume = currentMusicSource.volume;

        while (time < duration)
        {
            // Stop if either source is destroyed mid-fade
            if (nextMusicSource == null || currentMusicSource == null)
                yield break;

            time += Time.deltaTime;
            float t = time / duration;

            currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextMusicSource.volume = Mathf.Lerp(0f, startVolume, t);

            yield return null;
        }

        if (currentMusicSource != null)
        {
            currentMusicSource.Stop();
            currentMusicSource.volume = startVolume;
        }

        var temp = currentMusicSource;
        currentMusicSource = nextMusicSource;
        nextMusicSource = temp;

        Debug.Log("Crossfading to: " + newClip.name);
    }



    public void PlayUISound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, masterVolume * sfxVolume);
    }

    public void PlaySound(SoundData sound)
    {
        if (sound == null || sound.clip == null) return;
        sfxSource.clip = sound.clip;
        sfxSource.volume = masterVolume * sfxVolume * sound.volume;
        sfxSource.loop = sound.loop;
        sfxSource.Play();
    }

    public void ApplyVolumeSettings()
    {
        if (musicSourceA) musicSourceA.volume = masterVolume * musicVolume;
        if (musicSourceB) musicSourceB.volume = masterVolume * musicVolume;
        if (sfxSource) sfxSource.volume = masterVolume * sfxVolume;
        if (loopSource) loopSource.volume = masterVolume * sfxVolume;

        foreach (var src in extraSFXSources)
        {
            if (src != null)
                src.volume = masterVolume * sfxVolume;
        }
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ApplyVolumeSettings();
    }
    
    public float GetEffectiveSFXVolume()
{
    return masterVolume * sfxVolume;
}

}


