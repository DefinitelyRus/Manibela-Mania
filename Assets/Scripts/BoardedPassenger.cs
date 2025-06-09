using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BoardedPassenger {

	#region Fares

	public FareManager FareManager;

	/// <summary>
	/// The amount of fare that the passenger owes.
	/// </summary>
	public int FareOwed = 0;

	/// <summary>
	/// The amount of fare that the passenger has paid.
	/// </summary>
	public int FareToPay = 0;

	/// <summary>
	/// Indicates whether the passenger has fully paid their fare.
	/// </summary>
	public bool FullyPaid = false;

	/// <summary>
	/// Queues the passenger as the next one to pay their fare.
	/// </summary>
	public void Pay(bool debug = false) {
		if (debug) Debug.Log($"[BoardedPassenger] Paying fare: P{FareToPay} / P{FareOwed}");

		FareManager.QueuePayment(this, debug);

		ToDropOff = false;

		//TODO: Enable their payment popup bubble here. Maybe SFX too.
	}

	#endregion

	#region Dropping Off

	[Header("Dropping Off")]
	public float DropOffAtY;

	public bool ToDropOff = false;

	#endregion

	// The coin options that will be shown above the passenger's head
	public int[] CoinOptions = new int[4];

	public BoardedPassenger(FareManager fareManager, PassengerCarrier carrier, bool debug = false) {

		if (fareManager == null) {
			Debug.LogError("[BoardedPassenger] Given FareManager is null.");
			return;
		}

		if (carrier == null) {
			Debug.LogError("[BoardedPassenger] Given PassengerCarrier is null.");
			return;
		}

		FareManager = fareManager;

		// Generate a random fare between 1-20
		FareOwed = Random.Range(1, 21);
		
		// Generate 4 random coin options for the passenger to choose from
		for (int i = 0; i < 4; i++) {
			CoinOptions[i] = FareManager.Denominations[Random.Range(0, FareManager.Denominations.Length)];
		}

		// Set the fare to pay as the sum of the coin options
		FareToPay = 0;
		foreach (int coin in CoinOptions) {
			FareToPay += coin;
		}

		float dropOffDistance = Random.Range(carrier.DropOffDistanceMin, carrier.DropOffDistanceMax);
		DropOffAtY = carrier.transform.position.y + dropOffDistance;

		if (debug) Debug.Log($"[BoardedPassenger] Created passenger to drop off at Y: {DropOffAtY}.");
	}
}
