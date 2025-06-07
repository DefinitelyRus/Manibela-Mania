using UnityEngine;
using System.Collections;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource loopSource;  // Engine, idle, brake
    public AudioSource sfxSource;   // One-shot SFX
    public AudioSource musicSourceA;

    public AudioSource musicSourceB;

    private AudioSource currentMusicSource;

    private AudioSource nextMusicSource;


    public AudioClip[] MusicTracks; // Array of music tracks
    public AudioSource Music; // Music source for background music
    private Coroutine fadeCoroutine;



    void Start()
    {
        MusicTracks = Resources.LoadAll<AudioClip>("Music");

        currentMusicSource = musicSourceA;
        nextMusicSource = musicSourceB;

        if (MusicTracks.Length > 0)
            StartCoroutine(PlayMusicLoop());
    }


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySound(SoundData soundData)
    {
        PlaySound(soundData, Vector3.zero); // Use world origin or any default
    }

    public void PlaySound(SoundData soundData, Vector3 position)
    {
        if (soundData == null || soundData.clip == null) return;

        sfxSource.transform.position = position;
        sfxSource.clip = soundData.clip;
        sfxSource.volume = soundData.volume;
        sfxSource.loop = soundData.loop;
        sfxSource.Play();
    }

    public void CrossfadeSound(SoundData newSound, float duration)
    {
        if (newSound == null || newSound.clip == null) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToNewSound(newSound, duration));
    }

    private IEnumerator FadeToNewSound(SoundData newSound, float duration)
    {
        float time = 0f;
        float startVolume = loopSource.volume;

        // If we're switching clips, fade out current
        while (time < duration)
        {
            time += Time.deltaTime;
            loopSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        loopSource.clip = newSound.clip;
        loopSource.volume = 0f;
        loopSource.loop = newSound.loop;
        loopSource.Play();

        // Fade in new clip
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            loopSource.volume = Mathf.Lerp(0f, newSound.volume, time / duration);
            yield return null;
        }

        loopSource.volume = newSound.volume;
    }

    private IEnumerator PlayMusicLoop()
    {
        while (true)
        {
            if (!currentMusicSource.isPlaying)
            {
                AudioClip nextClip;

                // Choose a new clip different from current
                do
                {
                    nextClip = MusicTracks[Random.Range(0, MusicTracks.Length)];
                } while (nextClip == currentMusicSource.clip || nextClip.name == "Space Cruise");

                Debug.Log($"[SoundManager] Crossfading to: Ben Prunty - {nextClip.name}");

                // Start crossfade
                yield return StartCoroutine(CrossfadeTo(nextClip, 3f));  // 3s fade duration
            }

            yield return null;
        }
    }

private IEnumerator CrossfadeTo(AudioClip newClip, float duration) {
    nextMusicSource.clip = newClip;
    nextMusicSource.volume = 0f;
    nextMusicSource.Play();

    float time = 0f;
    float startVolume = currentMusicSource.volume;

    while (time < duration) {
        time += Time.deltaTime;
        float t = time / duration;

        currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
        nextMusicSource.volume = Mathf.Lerp(0f, startVolume, t);

        yield return null;
    }

    currentMusicSource.Stop();
    currentMusicSource.volume = startVolume;

    // Swap references
    AudioSource temp = currentMusicSource;
    currentMusicSource = nextMusicSource;
    nextMusicSource = temp;
}

}
