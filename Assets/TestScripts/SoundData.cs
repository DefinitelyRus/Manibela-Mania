using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public AudioClip clip;
    public float volume = 1f;
    public bool loop = false;
    
}
