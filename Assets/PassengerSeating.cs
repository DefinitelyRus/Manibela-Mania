using UnityEngine;

public class PassengerSeating : MonoBehaviour
{
    [System.Serializable]
    public class Seat
    {
        public string seatName;             // Optional for debug
        public SpriteRenderer seatRenderer; // The visual part to change sprite
        public bool occupied = false;       // Is seat taken?
        public string currentPassenger;     // Track which passenger is in this seat
    }

    public Seat[] seats = new Seat[6]; // Array for 6 seats in order

    public Sprite Char1Sit;
    public Sprite Char2Sit;
    public Sprite Char3Sit;
    public Sprite Char4Sit;
    public Sprite Char5Sit;
    public Sprite Char6Sit;

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
        ValidateSeats();
    }

    private void ValidateSeats()
    {
        if (seats == null || seats.Length != 6)
        {
            Debug.LogError("PassengerSeating must have exactly 6 seats configured!");
            return;
        }

        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == null)
            {
                Debug.LogError($"Seat {i} is null! Please configure all seats in the Inspector.");
                continue;
            }

            if (seats[i].seatRenderer == null)
            {
                Debug.LogError($"Seat {i} ({seats[i].seatName}) is missing its SpriteRenderer!");
            }
        }
    }

    /// <summary>
    /// Call this from Sakay.cs when a character is picked up.
    /// </summary>
    /// <param name="characterName">"Char1", "Char2", "Char3", etc.</param>
    public void AssignPassengerToSeat(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError("Cannot assign null or empty character name to seat!");
            return;
        }

        Sprite selectedSprite = GetSeatedSprite(characterName);
        if (selectedSprite == null)
        {
            Debug.LogError($"No seated sprite found for {characterName}");
            return;
        }

        // Find first available seat
        for (int i = 0; i < seats.Length; i++)
        {
            Seat seat = seats[i];
            if (seat == null)
            {
                Debug.LogError($"Seat {i} is null!");
                continue;
            }

            if (!seat.occupied)
            {
                // Assign the passenger to the seat
                seat.seatRenderer.sprite = selectedSprite;
                seat.occupied = true;
                seat.currentPassenger = characterName;

                Debug.Log($"{characterName} seated at {seat.seatName}");

                // Play the animation
                if (RearAnimAnimator != null)
                {
                    RearAnimAnimator.SetTrigger("PassengerSeated");
                    Debug.Log("RearAnim animation triggered for passenger seating.");
                }
                else
                {
                    Debug.LogWarning("RearAnimAnimator is not assigned. Cannot trigger animation.");
                }

                return;
            }
        }

        Debug.LogError($"No available seats found for {characterName}! All seats are occupied.");
    }

    /// <summary>
    /// Call this when a passenger is dropped off to clear their seat.
    /// </summary>
    /// <param name="characterName">The character being dropped off</param>
    public void ClearSeat(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError("Cannot clear seat for null or empty character name!");
            return;
        }

        for (int i = 0; i < seats.Length; i++)
        {
            Seat seat = seats[i];
            if (seat != null && seat.currentPassenger == characterName)
            {
                seat.seatRenderer.sprite = null;
                seat.occupied = false;
                seat.currentPassenger = null;
                Debug.Log($"Cleared seat {seat.seatName} for {characterName}");
                return;
            }
        }

        Debug.LogWarning($"No seat found to clear for {characterName}");
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
            case "Char5":
                return Char5Sit;
            case "Char6":
                return Char6Sit;
            default:
                Debug.LogError($"Unknown character type: {characterName}");
                return null;
        }
    }
}
