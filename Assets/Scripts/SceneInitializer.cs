using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
   public AudioSource engineSource;
    public AudioSource sfxSource;

    void Start()
    {

        if (!GameObject.Find("Jeep").TryGetComponent<VehicleMovement>(out var vehicle))
        {
            Debug.LogWarning("VehicleMovement not found in scene.");
            return;
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager.Instance is null.");
            return;
        }

        //vehicle.engineSource = SoundManager.Instance.loopSource;
        //vehicle.sfxSource = SoundManager.Instance.sfxSource;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SwitchToGameMusic();
        }
        
        if (SoundManager.Instance != null && engineSource != null)
		{
			SoundManager.Instance.RegisterExtraSFXSource(engineSource);
		}
    }
}
