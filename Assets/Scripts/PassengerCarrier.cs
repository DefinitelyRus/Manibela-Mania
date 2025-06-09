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

	public VehicleMovement JeepM;

	public PassengerSeating PassengerSeating;

	#endregion

	#region Infil & Exfil

	/// <summary>
	/// The maximum number of passengers that the carrier can hold.
	/// </summary>
	[Header("Infil & Exfil")]
	public int MaximumPassengers = 6;

	/// <summary>
	/// The position where the passengers will spawn at when they're ejected.
	/// </summary>
	public Vector2 EntranceLocation;

	public float MinPayWait = 3f;

	public float MaxPayWait = 5f;

	public float DropOffXLeft = 71f;

	public float DropOffXRight = 72f;
	
	public float DropOffXOffset = 0.5f;

	public float DropOffDistanceMin = 500f;

	public float DropOffDistanceMax = 5000f;

	public float DropOffWithin = 800f;

	public List<BoardedPassenger> Passengers = new();

	/// <summary>
	/// Adds a passenger to the carrier if there is space.
	/// </summary>
	/// <param name="passenger">The passenger to add.</param>
	/// <returns>True if the passenger was added, false if the carrier is full.</returns>
	public bool AddPassenger(BoardedPassenger passenger)
	{
		if (Passengers.Count >= MaximumPassengers)
		{
			Debug.LogWarning("[PassengerCarrier] Cannot add passenger - at capacity!");
			return false;
		}

		// Check if this character type is already seated
		if (PassengerSeating != null)
		{
			// Get the character number from the passenger's index
			int passengerIndex = Passengers.Count;
			string characterName = $"Char{passengerIndex + 1}";

			// Check if this character is already seated
			foreach (PassengerSeating.Seat seat in PassengerSeating.seats)
			{
				if (seat.occupied && seat.currentPassenger == characterName)
				{
					Debug.LogWarning($"[PassengerCarrier] Cannot add {characterName} - already seated!");
					return false;
				}
			}
		}

		Passengers.Add(passenger);
		return true;
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
		BoardedPassenger newPassenger = new(FareManager, this, debug);

		if (AddPassenger(newPassenger)) {
			StartCoroutine(PaymentWait(newPassenger, Random.Range(MinPayWait, MaxPayWait), debug));
			if (debug) Debug.Log($"[PassengerCarrier] Abducted passenger successfully. ({Passengers.Count}/{MaximumPassengers})");
		} else {
			EjectPassenger(newPassenger, EntranceLocation);
			if (debug) Debug.Log($"[PassengerCarrier] Abducted passenger but could not add to carrier. Ejecting passenger.");
		}
	}

	/// <summary>
	/// Waits for a specified number of seconds before paying the passenger's fare.
	/// </summary>
	private IEnumerator PaymentWait(BoardedPassenger passenger, float seconds, bool debug = false) {
		if (debug) Debug.Log($"[PassengerCarrier] Waiting {seconds} seconds before paying...");
		yield return new WaitForSeconds(seconds);
		passenger.Pay(debug);
	}

	/// <summary>
	/// Checks if any passengers need to be dropped off as soon as possible.
	/// </summary>
	private void CheckPassengerDropOffs(bool debug = false) {
		try {
			foreach (BoardedPassenger passenger in Passengers) {
				bool toDropOff = transform.position.y > passenger.DropOffAtY;
				bool missedStop = transform.position.y > passenger.DropOffAtY + DropOffWithin;

				if (toDropOff) {
					if (debug) Debug.Log($"[PassengerCarrier] Passenger {passenger} wants to be dropped off.");
					RequestFullStop(passenger, debug);
				}

				if (missedStop) {
					// Add a simple penalty for missing stops
					FareManager.TotalPenalty += 50;
					if (debug) Debug.Log($"[PassengerCarrier] Missed stop! Penalty: P{FareManager.TotalPenalty}");
				}
			}
		}
		catch { }
		//NOTE: An exception is thrown when Passengers is modified during iteration.
		//Since we're removing passengers from the list, we expect this to happen.
	}

	/// <summary>
	/// Requests a full stop for the vehicle to drop off a passenger.
	/// This removes the passenger from the vehicle and spawns them at the drop-off point.
	/// It will also remove them from <see cref="Passengers"/>.
	/// </summary>
	/// <param name="passenger"></param>
	/// <param name="debug"></param>
	private void RequestFullStop(BoardedPassenger passenger, bool debug = false) {

		bool atLeftmostLane = transform.position.x <= DropOffXLeft;
		bool atRightmostLane = transform.position.x >= DropOffXRight;
		bool isPulledOver = atLeftmostLane || atRightmostLane;

		float dropOffX = 0;
		if (atLeftmostLane) dropOffX = DropOffXLeft - DropOffXOffset;
		else if (atRightmostLane) dropOffX = DropOffXRight + DropOffXOffset;

		Vector2 dropOffTo = new(dropOffX, transform.position.y);

		if (JeepM.Speed == 0 && isPulledOver) {
			if (debug) Debug.Log($"[PassengerCarrier] Jeep stopped. Ejecting passenger {passenger}.");
			EjectPassenger(passenger, dropOffTo, debug);
		}
	}

	/// <summary>
	/// Spawns the passenger prefab at the drop-off point and removes the passenger from the vehicle.
	/// </summary>
	/// <param name="passenger">The passenger to yeet.</param>
	public void EjectPassenger(BoardedPassenger passenger, Vector2 dropoffPoint, bool debug = false) {
		// Get the passenger's index before removing them
		int passengerIndex = Passengers.IndexOf(passenger);
		
		// Remove the passenger from the carrier
		RemovePassenger(passenger);

		// Clear the passenger's seat
		if (PassengerSeating != null) {
			PassengerSeating.ClearSeat($"Char{passengerIndex + 1}");
		}

		// Spawn the passenger prefab at the drop-off point
		GameObject someDude = Instantiate(PassengerPrefab, dropoffPoint, Quaternion.identity);

		if (debug) Debug.Log($"[PassengerCarrier] Ejected passenger from seat {passengerIndex + 1}.");
	}

	#endregion

	#region Unity Callbacks

	private void Start() {
		if (FareManager == null) {
			Debug.LogError("[PassengerCarrier] FareManager not found in the scene.");
		}
		if (PassengerSeating == null) {
			Debug.LogError("[PassengerCarrier] PassengerSeating not found in the scene.");
		}
	}

	private void Update() {
		CheckPassengerDropOffs(true);
	}

	#endregion
}
