using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;

public class Sakay : MonoBehaviour
{
    public string characterName = "char1"; // Set in Inspector or dynamically
    public GameObject timerBarPrefab;      // Assign the TimerBarUI prefab in Inspector

    [SerializeField] private Vector3 timerBarOffset = new(0, 1.5f, 0); // Editable in Inspector

    private float Timer = 0f;

    private GameObject TimerBar;
    private Slider TimerSlider;
    private Camera Camera;

	private PassengerCarrier PassengerCarrier;

	private readonly float TimerDuration = 2f;

	private void Start()
    {
        Camera = Camera.main;

		PassengerCarrier = GameObject.Find("Jeep").GetComponent<PassengerCarrier>();

		if (timerBarPrefab != null)
        {
            // Instantiate and parent to this character
            TimerBar = Instantiate(timerBarPrefab, transform);

            // Set localPosition to zero so we control world position explicitly in Update
            TimerBar.transform.localPosition = Vector3.zero;
            TimerSlider = TimerBar.GetComponentInChildren<Slider>();
            TimerBar.SetActive(false);
        }

        // Convert characterName to proper format (e.g., "char1" to "Char1")
        characterName = "Char" + characterName[4..];
    }

    private void Update() {
		//wtf does this do?
        if (TimerBar != null) TimerBar.transform.SetPositionAndRotation(transform.position + timerBarOffset, Quaternion.LookRotation(Camera.transform.forward));
    }

	private void OnboardPassenger() {
		Timer += Time.deltaTime;
		TimerSlider.value = Timer / TimerDuration;

		if (Timer >= TimerDuration) {
			Debug.Log($"[Sakay] Boarding passenger.");
			GlobalCounter.Increment(characterName);
			GlobalCounter.passengernum += 1;

			//Assign the passenger to a seat
			//TODO: Treat all seats as generic; do not lock to a specific character.
			if (PassengerSeating.Instance != null) PassengerSeating.Instance.AssignPassengerToSeat(characterName);
			else Debug.LogError("PassengerSeating.Instance is null! Make sure there is a PassengerSeating component in the scene.");

			//Add this passenger to the carrier.
			PassengerCarrier.AbductPassenger(true);

			//Destroy the evidence.
			Destroy(TimerBar);
			Destroy(gameObject);
		}
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Timer = 0f;
            TimerBar.SetActive(true);
            TimerSlider.value = 0f;
        }
    }

	private void OnTriggerStay2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {

			//Exit on full.
			if (PassengerCarrier.Passengers.Count >= PassengerCarrier.MaximumPassengers) {
				Debug.Log("[Sakay] Full; cannot pick up any more.");
				return;
			}

			OnboardPassenger();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Timer = 0f;
            TimerSlider.value = 0f;
            TimerBar.SetActive(false);
        }
    }
}
