using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerCarrier : MonoBehaviour
{
	#region Game Objects & Components

	/// <summary>
	/// The fare manager that handles the fares of the passengers in the carrier.
	/// </summary>
	[Header("Game Objects & Components")]
	public FareManager FareManager;

	/// <summary>
	/// The prefab for the in-world passenger object that will be instantiated
	/// when a passenger is ejected back into the game world.
	/// <br/><br/>
	/// Not to be confused with the <see cref="BoardedPassenger"/> class.
	/// </summary>
	public GameObject PassengerPrefab;

	#endregion

	#region Infil & Exfil

	/// <summary>
	/// The maximum number of passengers that the carrier can hold.
	/// </summary>
	[Header("Infil & Exfil")]
	public int MaximumPassengers = 16;

	/// <summary>
	/// The position where the passengers will spawn at when they're ejected.
	/// </summary>
	public Vector2 EntranceLocation;

	/// <summary>
	/// The direction the passenger will face after being ejected from the vehicle.
	/// </summary>
	public float EjectAngle = 180f;

	public List<BoardedPassenger> Passengers { get; private set; } = new();

	/// <summary>
	/// Adds a passenger to the carrier if there is space.
	/// </summary>
	/// <param name="passenger">The passenger to add.</param>
	/// <returns>True if the passenger was added, false if the carrier is full.</returns>
	public void AddPassenger(BoardedPassenger passenger, bool debug = false) {
		if (Passengers.Count >= MaximumPassengers) {
			if (debug) Debug.Log($"[PassengerCarrier] Over capacity!");
		}

		Passengers.Add(passenger);

		if (debug) Debug.Log($"[PassengerCarrier] Passenger added. ({Passengers.Count}/{MaximumPassengers})");
	}

	/// <summary>
	/// Removes a passenger from the carrier.
	/// </summary>
	/// <param name="passenger">The passenger to remove.</param>
	public void RemovePassenger(BoardedPassenger passenger, bool debug = false) {
		Passengers.Remove(passenger);

		if (debug) Debug.Log($"[PassengerCarrier] Passenger removed. ({Passengers.Count}/{MaximumPassengers})");
	}

	/// <summary>
	/// Creates a new passenger object and adds it to the carrier.
	/// </summary>
	public void AbductPassenger(bool debug = false) {
		BoardedPassenger newPassenger = new(FareManager);
		AddPassenger(newPassenger, debug);

		StartCoroutine(PaymentWait(newPassenger, Random.Range(3f, 5f), debug));

		if (Passengers.Count > MaximumPassengers) {
			EjectPassenger(newPassenger);
			if (debug) Debug.Log($"[PassengerCarrier] Abducted passenger but exceeded capacity. Ejecting passenger.");
		}

		else if (debug) Debug.Log($"[PassengerCarrier] Abducted passenger successfully. ({Passengers.Count}/{MaximumPassengers})");
	}

	private IEnumerator PaymentWait(BoardedPassenger passenger, float seconds, bool debug = false) {
		yield return new WaitForSeconds(seconds);
		Debug.Log("[PassengerCarrier] Passenger is ready to pay fare.");
		passenger.Pay();
	}

	/// <summary>
	/// Spawns the passenger prefab at the drop-off point and removes the passenger from the vehicle.
	/// </summary>
	/// <param name="passenger">The passenger to yeet.</param>
	public void EjectPassenger(BoardedPassenger passenger, bool debug = false) {
		RemovePassenger(passenger);

		GameObject someDude = Instantiate(PassengerPrefab, EntranceLocation, Quaternion.Euler(0, 0, EjectAngle));

		//TODO: Manually configure the passenger's destination here. Example:
		//HumanMovement someDudeScript = someDude.GetComponent<HumanMovement>();
		//someDudeScript.Destination = EntranceLocation;

		if (debug) Debug.Log($"[PassengerCarrier] Ejected passenger.");
	}

	#endregion

	#region Unity Callbacks

	private void Start() {
		if (FareManager == null) {
			Debug.LogError("[PassengerCarrier] FareManager not found in the scene.");
		}
	}

	#endregion
}
