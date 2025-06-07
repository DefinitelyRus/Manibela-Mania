using UnityEngine;
<<<<<<< Updated upstream

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
=======
using System;
using Random = System.Random;

public class Passenger : MonoBehaviour
{
	public int FareOwed = 0;

	public static int MinimumFare = 11;

	public static int MaximumFare = 30;

	public int FarePaid = 0;

	public int[] FairPaidInCash;

	public static int MinimumPay = 1;

	public static int MaximumPay = 1000;

	public int FareChange = 0;

	private void Start() {
		
	}

	/// <summary>
	/// Calculates a biased random value between min and max, biased towards the average.
	/// </summary>
	/// <param name="min">The minimum value to return.</param>
	/// <param name="max"></param>
	/// <param name="average"></param>
	/// <returns></returns>
	public static double BiasedRandom( double min, double max, double average) {
		Random random = new();

		double range = max - min;
		double midpoint = (min + max) / 2;

		//Determine bias direction
		bool biasRight = average > midpoint;

		//Bias strength: closer to min or max means stronger skew
		double distance = Math.Abs(average - midpoint) / (range / 2);
		double exponent = biasRight ? (1.0 - distance) * 2 + 1 : (1.0 + distance) * 2 + 1;

		//Generate biased number
		double random01 = random.NextDouble();
		double skewed = biasRight ? Math.Pow(random01, 1.0 / exponent) : 1 - Math.Pow(1 - random01, 1.0 / exponent);

		return min + skewed * range;
	}

	public static int[] Denominations = new int[] { 1, 5, 10, 20, 50, 100, 200, 500, 1000 };

	public static int RoundToBill(int value) {
		//Round to the nearest bill denomination
		if (value < 50) {
			return value;
		}

		//For values 50 and above, round to the nearest denomination
		for (int i = Denominations.Length - 1; i >= 0; i--) {
			if (value >= Denominations[i]) {
				return Denominations[i];
			}
		}
	}

>>>>>>> Stashed changes
}
