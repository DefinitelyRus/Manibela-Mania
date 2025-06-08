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
	/// The denominations of coins that the passenger has paid in cash.
	/// </summary>
	public int[] FairToPayInCash;

	/// <summary>
	/// The change that the passenger expects to receive.
	/// </summary>
	public int ExpectedChange = 0;

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

	/// <summary>
	/// Receives a change from the player.
	/// </summary>
	/// <param name="change">How much change to receive.</param>
	/// <param name="debug">Whether to print logs to console.</param>
	public void ReceiveChange(int change, bool debug = false) {
		if (change >= ExpectedChange) {
			if (debug) Debug.Log($"[BoardedPassenger] Completed change: P{change} / P{ExpectedChange}. Fully paid and to drop off!");
			FullyPaid = true;
			ToDropOff = true;

			return;
		}

		else {
			if (debug) Debug.Log($"[BoardedPassenger] Insufficient change: P{change} / P{ExpectedChange}");
			FullyPaid = false;

			ExpectedChange -= change;
		}
	}

	#endregion

	#region Dropping Off

	[Header("Dropping Off")]
	public float DropOffAtY;

	public bool ToDropOff = false;

	#endregion

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

		FareOwed = Random.Range(FareManager.MinimumFare, FareManager.MaximumFare + 1);
		FareToPay = Random.Range(FareOwed, FareManager.MaximumPay + 1);
		FairToPayInCash = FareManager.RoundToCoins(FareManager.RoundToBill(FareToPay));

		float dropOffDistance = Random.Range(carrier.DropOffDistanceMin, carrier.DropOffDistanceMax);
		DropOffAtY = carrier.transform.position.y + dropOffDistance;

		if (debug) Debug.Log($"[BoardedPassenger] Created passenger to drop off at Y: {DropOffAtY}.");
	}
}
