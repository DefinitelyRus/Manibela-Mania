using UnityEngine;

public class PassengerSeating : MonoBehaviour
{
    [System.Serializable]
    public class Seat
    {
        public string seatName;             // Optional for debug
        public SpriteRenderer seatRenderer; // The visual part to change sprite
        public bool occupied = false;       // Is seat taken?
    }

    public Seat[] seats = new Seat[6]; // Array for 6 seats in order

    public Sprite Char1Sit;
    public Sprite Char2Sit;
    public Sprite Char3Sit;
    public Sprite Char4Sit;

    // NEW: Reference to the Animator on the "RearAnim" GameObject
    public Animator RearAnimAnimator;

    public static PassengerSeating Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Call this from Sakay.cs when a character is picked up.
    /// </summary>
    /// <param name="characterName">"Char1", "Char2", "Char3", or "Char4"</param>
    public void AssignPassengerToSeat(string characterName)
    {
        Sprite selectedSprite = GetSeatedSprite(characterName);

        if (selectedSprite == null)
        {
            Debug.LogWarning($"No seated sprite found for {characterName}");
            return;
        }

        // Flag to track if a seat was found and assigned (no longer strictly needed for this logic, but kept for clarity)
        // bool seatAssigned = false; // Original purpose was to trigger 'no seats available' logic.

        for (int i = 0; i < seats.Length; i++)
        {
            Seat seat = seats[i];

            if (!seat.occupied)
            {
                seat.seatRenderer.sprite = selectedSprite;
                seat.occupied = true;
                // seatAssigned = true; // Original purpose was to trigger 'no seats available' logic.

                // Removed flipping behavior
                Debug.Log($"{characterName} seated at {seat.seatName}");

                // MOVED: Play the animator on RearAnim if it's assigned and a sprite was successfully selected
                // This code now runs ONLY when a seat is successfully assigned.
                if (RearAnimAnimator != null)
                {
                    // Using SetTrigger with a parameter named "PassengerSeated"
                    RearAnimAnimator.SetTrigger("PassengerSeated");
                    Debug.Log("RearAnim animation triggered because a passenger was successfully seated.");
                }
                else
                {
                    Debug.LogWarning("RearAnimAnimator is not assigned. Cannot trigger animation.");
                }

                return; // Exit the method after assigning a seat and triggering animation
            }
        }

        // If the loop finishes and no seat was found, it means there were no available seats.
        // The animation trigger for "no seats" is now removed from here as per your request.
        Debug.LogWarning("No available seats to assign passenger.");
    }

    private Sprite GetSeatedSprite(string characterName)
    {
        switch (characterName)
        {
            case "Char1":
                return Char1Sit;
            case "Char2":
                return Char2Sit;
            case "Char3":
                return Char3Sit;
            case "Char4":
                return Char4Sit;
            default:
                return null;
        }
    }
}
