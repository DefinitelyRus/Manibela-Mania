using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Sakay : MonoBehaviour
{
    public string characterName = "char1"; // Set in Inspector or dynamically
    public GameObject timerBarPrefab;      // Assign the TimerBarUI prefab in Inspector

    [SerializeField] private Vector3 timerBarOffset = new Vector3(0, 1.5f, 0); // Editable in Inspector

    private float timer = 0f;
    private bool playerInside = false;
    private float updateTimer = 0f; // Timer for updating passenger status

    public static bool passengerFull = false;
    public static Dictionary<string, int> passengerDropOffs = new Dictionary<string, int>(); // Tracks which zone each passenger needs to be dropped off at
    public static HashSet<string> activePassengerTypes = new HashSet<string>(); // Tracks which passenger types are currently in the vehicle
    public static int currentArea = 0; // Tracks the current area number

    private GameObject timerBarInstance;
    private Slider timerSlider;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (timerBarPrefab != null)
        {
            // Instantiate and parent to this character
            timerBarInstance = Instantiate(timerBarPrefab, transform);
            // Set localPosition to zero so we control world position explicitly in Update
            timerBarInstance.transform.localPosition = Vector3.zero;
            timerSlider = timerBarInstance.GetComponentInChildren<Slider>();
            timerBarInstance.SetActive(false); // start hidden
        }

        // Convert characterName to proper format (e.g., "char1" to "Char1")
        characterName = "Char" + characterName.Substring(4);
    }

    private void Update()
    {
        if (timerBarInstance != null)
        {
            // Update position relative to character's world position + offset
            timerBarInstance.transform.position = transform.position + timerBarOffset;

            // Make the timer bar face the camera
            timerBarInstance.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
        }

        // Update passenger status every second
        updateTimer += Time.deltaTime;
        if (updateTimer >= 1f)
        {
            updateTimer = 0f;
            UpdatePassengerStatus();
        }

        if (playerInside && GlobalCounter.passengernum < 6)
        {
            timer += Time.deltaTime;

            if (timerSlider != null)
            {
                timerSlider.value = timer / 2f; // Normalize to 0-1 range for 2 second pickup
            }

            if (timer >= 2f)
            {
                // Check if this passenger type is already in the vehicle
                if (activePassengerTypes.Contains(characterName))
                {
                    Debug.Log($"Cannot pick up {characterName} - already in vehicle");
                    playerInside = false;
                    timer = 0f;
                    if (timerBarInstance != null)
                    {
                        timerSlider.value = 0f;
                        timerBarInstance.SetActive(false);
                    }
                    return;
                }

                Debug.Log($"Player stayed for 2 seconds. Boarding passenger {characterName}.");
                GlobalCounter.Increment(characterName);
                GlobalCounter.passengernum += 1;

                // Assign a random drop-off range (10-40)
                int dropOffRange = Random.Range(10, 41);
                passengerDropOffs[characterName] = dropOffRange;
                activePassengerTypes.Add(characterName);
                Debug.Log($"Passenger {characterName} needs to be dropped off after {dropOffRange} areas. They can get off at any drop-off zone after that point.");

                // Assign the passenger to a seat
                if (PassengerSeating.Instance != null)
                {
                    PassengerSeating.Instance.AssignPassengerToSeat(characterName);
                }
                else
                {
                    Debug.LogWarning("PassengerSeating.Instance is null! Make sure there is a PassengerSeating component in the scene.");
                }

                Debug.Log($"Total passengers: {GlobalCounter.passengernum}");

                if (GlobalCounter.passengernum >= 6)
                {
                    passengerFull = true;
                    Debug.Log("Passenger limit now reached.");
                }

				if (!GameObject.Find("Jeep").TryGetComponent<PassengerCarrier>(out var carrier)) Debug.LogError("[Sakay] Passenger Carrier not found on Jeep object.");
				else carrier.AbductPassenger(true);

				Destroy(timerBarInstance);
                Destroy(gameObject);
            }
        }
    }

    private void UpdatePassengerStatus()
    {
        if (passengerDropOffs.Count == 0)
        {
            return;
        }

        foreach (var passenger in passengerDropOffs)
        {
            int remainingAreas = passenger.Value - currentArea;
            if (remainingAreas <= 0)
            {
                //Debug.Log($"Passenger {passenger.Key} has reached their destination. They can get off at any drop-off zone.");
            }
            else
            {
                //Debug.Log($"Passenger {passenger.Key} needs {remainingAreas} more areas to reach destination. They can get off at any drop-off zone after that.");
            }
        }
    }

    // Call this method from the Spawner when a new area is spawned
    public static void IncrementArea()
    {
        currentArea++;
        Debug.Log($"Entered area {currentArea}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GlobalCounter.passengernum >= 6)
            {
                passengerFull = true;
                Debug.Log("Passenger limit reached. Cannot pick up more.");
                return;
            }

            Debug.Log($"Player entered 2D trigger for {characterName}!");
            playerInside = true;
            timer = 0f;

            if (timerBarInstance != null)
            {
                timerBarInstance.SetActive(true);
                if (timerSlider != null)
                {
                    timerSlider.value = 0f;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited 2D trigger for {characterName}!");
            playerInside = false;
            timer = 0f;

            if (timerBarInstance != null)
            {
                timerSlider.value = 0f;
                timerBarInstance.SetActive(false);
            }
        }
    }
}
