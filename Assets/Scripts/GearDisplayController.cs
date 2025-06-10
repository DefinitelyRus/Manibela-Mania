using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnifiedGearUIController : MonoBehaviour, IPointerClickHandler
{
    public VehicleMovement car;

    public Image gearImage;

    public Sprite[] manualSprites; // 0: Neutral, 1: Reverse, 2: Gear 1, 3: Gear 2, 4: Gear 3
    public Sprite[] autoSprites;   // 0: Neutral, 1: Reverse, 2: Gear 1, 3: Gear 2, 4: Gear 3

    private int lastGear = 999;
    private bool lastAuto = false;

    void Update()
    {
        if (car == null || gearImage == null) return;

        if (car.UseAutoShift)
        {
            if (car.CurrentGear != lastGear || !lastAuto)
            {
                gearImage.sprite = GetSprite(autoSprites, car.CurrentGear);
                lastGear = car.CurrentGear;
                lastAuto = true;
            }
        }
        else
        {
            if (car.CurrentGear != lastGear || lastAuto)
            {
                gearImage.sprite = GetSprite(manualSprites, car.CurrentGear);
                lastGear = car.CurrentGear;
                lastAuto = false;
            }
        }
    }

    Sprite GetSprite(Sprite[] sprites, int gear)
    {
        return gear switch
        {
            0 => sprites.Length > 0 ? sprites[0] : null, // Neutral
           -1 => sprites.Length > 1 ? sprites[1] : null, // Reverse
            1 => sprites.Length > 2 ? sprites[2] : null,
            2 => sprites.Length > 3 ? sprites[3] : null,
            3 => sprites.Length > 4 ? sprites[4] : null,
            _ => sprites.Length > 0 ? sprites[0] : null,
        };
    }

    // This gets called when gearImage is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (car == null) return;

        car.UseAutoShift = !car.UseAutoShift;
        Debug.Log("Shift mode toggled. Now AutoShift is " + car.UseAutoShift);
    }
}
