using UnityEngine;

public class Passenger : MonoBehaviour
{
	#region Fares

	public FareManager FareManager;

	/// <summary>
	/// The amount of fare that the passenger owes.
	/// </summary>
	public int FareOwed = 0;

	/// <summary>
	/// The amount of fare that the passenger has paid.
	/// </summary>
	public int FarePaid = 0;

	/// <summary>
	/// The denominations of coins that the passenger has paid in cash.
	/// </summary>
	public int[] FairPaidInCash;

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

	private void Start() {

		#region Fares

		FareOwed = (int) FareManager.BiasedRandom(FareManager.MinimumFare, FareManager.MaximumFare, 20);

		int randomPay = (int) FareManager.BiasedRandom(FareManager.MinimumPay, FareManager.MaximumPay, 30);
		int paid = FareManager.RoundToBill(randomPay);
		FarePaid = paid;

		ExpectedChange = FareOwed - FarePaid;

		int[] paidCoins = paid < 50 ? FareManager.RoundToCoins(paid, true) : new int[] { paid };
		FairPaidInCash = paidCoins;

		#endregion
	}
}
