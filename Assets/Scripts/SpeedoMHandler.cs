using UnityEngine;
using UnityEngine.UI;

public class SpeedometerController : MonoBehaviour
{
    public Image speedometerImage;       // UI Image showing the speedometer
    public Sprite[] speedometerFrames;   // Array of sprite frames (like your GIF)
    
    public VehicleMovement car;          // Reference to your car movement script
    public float maxDisplaySpeed = 180f; // Max speed shown by your speedometer

    void Update()
    {
        if (car == null || speedometerFrames == null || speedometerFrames.Length == 0)
            return;

        float normalizedSpeed = Mathf.Clamp01(car.Speed / maxDisplaySpeed);
        int frameIndex = Mathf.FloorToInt(normalizedSpeed * (speedometerFrames.Length - 1));

        speedometerImage.sprite = speedometerFrames[frameIndex];
    }
}
