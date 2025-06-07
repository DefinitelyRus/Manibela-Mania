using UnityEngine;
using System.Collections;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource loopSource;  // Engine, idle, brake
    public AudioSource sfxSource;   // One-shot SFX
    public AudioSource musicSource;

    private Coroutine fadeCoroutine;

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
}
