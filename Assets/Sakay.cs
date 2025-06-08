using UnityEngine;
using UnityEngine.UI;

public class Sakay : MonoBehaviour
{
    public string characterName = "char1"; // Set in Inspector or dynamically
    public GameObject timerBarPrefab;      // Assign the TimerBarUI prefab in Inspector

    [SerializeField] private Vector3 timerBarOffset = new Vector3(0, 1.5f, 0); // Editable in Inspector

    private float timer = 0f;
    private bool playerInside = false;

    public static bool passengerFull = false;

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

        if (playerInside && GlobalCounter.passengernum < 6)
        {
            timer += Time.deltaTime;

            if (timerSlider != null)
            {
                timerSlider.value = timer;
            }

            if (timer >= 2f)
            {
                Debug.Log("Player stayed for 2 seconds. Boarding passenger.");
                GlobalCounter.Increment(characterName);
                GlobalCounter.passengernum += 1;

                // Assign passenger to seat
                if (PassengerSeating.Instance != null)
                {
                    PassengerSeating.Instance.AssignPassengerToSeat(characterName);
                }
                else
                {
                    Debug.LogWarning("PassengerSeating.Instance is null! Check setup.");
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

            Debug.Log("Player entered 2D trigger!");
            playerInside = true;
            timer = 0f;

            if (timerBarInstance != null)
                timerBarInstance.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited 2D trigger!");
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
