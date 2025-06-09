using UnityEngine;
using UnityEngine.UI;

public class GearDisplayController : MonoBehaviour
{
    public Image gearImage;             // UI image for gear display
    public Sprite[] gearSprites;        // Indexed gear sprites: N, R, 1, 2, 3...
    public VehicleMovement car;         // Reference to your vehicle movement script

    void Update()
    {
        if (car == null || gearSprites == null || gearSprites.Length == 0)
            return;

        int gearIndex = GetGearSpriteIndex(car.CurrentGear);
        if (gearIndex >= 0 && gearIndex < gearSprites.Length)
            gearImage.sprite = gearSprites[gearIndex];
    }

    int GetGearSpriteIndex(int currentGear)
    {
        return currentGear switch
        {
            0 => 0,  // Neutral
            -1 => 1, // Reverse
            1 => 2,
            2 => 3,
            3 => 4,
            _ => 0   // fallback to Neutral
        };
    }
}
