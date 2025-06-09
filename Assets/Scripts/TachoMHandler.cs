using UnityEngine;
using UnityEngine.UI;

public class MeterHandler : MonoBehaviour
{
    public Image tachometerImage;        // UI Image showing the tachometer
    public Sprite[] tachometerFrames;    // Your sliced GIF frames

    public VehicleMovement car;          // Reference to your movement script

    void Update()
    {
        if (car == null || tachometerFrames == null || tachometerFrames.Length == 0)
            return;

        float normalized = Mathf.Clamp01(Mathf.Abs(car.Tachometer)); // In case of reverse

        int frame = Mathf.FloorToInt(normalized * (tachometerFrames.Length - 1));
        tachometerImage.sprite = tachometerFrames[frame];
    }
}
