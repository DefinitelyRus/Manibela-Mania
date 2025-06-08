using UnityEngine;

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
	public void Pay() {
		FareManager.QueuePayment(this);

		//TODO: Enable their payment popup bubble here. Maybe SFX too.
	}

	/// <summary>
	/// Receives a change from the player.
	/// </summary>
	/// <param name="change">How much change to receive.</param>
	/// <param name="debug">Whether to print logs to console.</param>
	public void ReceiveChange(int change, bool debug = false) {
		if (change >= ExpectedChange) {
			if (debug) Debug.Log($"[Passenger] Completed change: P{change} / P{ExpectedChange}");
			FullyPaid = true;

			return;
		}

		else {
			if (debug) Debug.Log($"[Passenger] Insufficient change: P{change} / P{ExpectedChange}");
			FullyPaid = false;

			ExpectedChange -= change;
		}
	}

	#endregion

	#region Dropping Off

	/// <summary>
	/// Chance of the passenger dropping off at the next stop.
	/// </summary>
	[Header("Dropping Off")]
	public int DropoffNextChance = 10;

	/// <summary>
	/// Indicates whether the passenger will drop off at the next stop.
	/// </summary>
	public bool WillDropOffNext = false;

	/// <summary>
	/// Decides whether the passenger will drop off at the next stop based on a random chance.
	/// </summary>
	/// <param name="debug"></param>
	public void DecideDropoff(bool debug = false) {
		int rng = Random.Range(0, 100);

		if (rng > DropoffNextChance) {
			WillDropOffNext = true;
			
			if (debug) Debug.Log($"[Passenger] Passenger will drop off at next stop.");
		}
	}

	#endregion

	public BoardedPassenger(FareManager fareManager) {
		FareManager = fareManager;

		if (FareManager == null) {
			Debug.LogError("[Passenger] FareManager not found in the scene.");

			return;
		}

		FareOwed = Random.Range(FareManager.MinimumFare, FareManager.MaximumFare + 1);
		FareToPay = Random.Range(FareManager.MinimumPay, FareManager.MaximumPay + 1);
		FairToPayInCash = FareManager.RoundToCoins(FareManager.RoundToBill(FareToPay));
	}
}
