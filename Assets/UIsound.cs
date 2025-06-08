using UnityEngine;

public class UISoundHandler : MonoBehaviour
{
    [Range(0f, 1f)] public float volume = 1f;

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUISound(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }
    }
}
