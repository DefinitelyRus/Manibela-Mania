using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class FareManager : MonoBehaviour {

	#region Moneys

	/// <summary>
	/// How much fare the jeepney has collected so far.
	/// </summary>
	[Header("Money")]
	public int Balance = 0;

	public int MinimumFare = 11;

	public int MaximumFare = 30;

	public int MinimumPay = 11;

	public int MaximumPay = 1000;

	
	#endregion

	#region Passengers

	/// <summary>
	/// The next passenger that is queued for payment.
	/// </summary>
	public Queue<BoardedPassenger> PassengerQueue { get; private set; } = new();

	/// <summary>
	/// The current passenger that is being processed for payment.
	/// </summary>
	public BoardedPassenger CurrentPassenger { get; private set; }

	#endregion

	#region Payment

	public SpriteRenderer RearviewSprite;

	public Color RearviewColor = Color.green;

	/// <summary>
	/// Queues the passenger as the next one to pay their fare.
	/// <br/><br/>
	/// This function is called when the passenger wants to pay their fare.
	/// </summary>
	public void QueuePayment(BoardedPassenger passenger, bool debug = false) {
		if (passenger == null) {
			Debug.LogError("[FareManager] Cannot queue payment for a null passenger.");
			return;
		}

		if (debug) Debug.Log($"[FareManager] Queued passenger for payment.");

		PassengerQueue.Enqueue(passenger);

		RearviewSprite.color = RearviewColor;
	}

	/// <summary>
	/// Sets the current passenger as the current passenger and removes them from the queue.
	/// </summary>
	/// <param name="debug">Whether to print logs to console.</param>
	public void AcceptPayment(bool debug = false) {
		if (PassengerQueue.Count == 0) {
			Debug.LogError("[FareManager] No passengers in queue to accept payment from.");
			return;
		}

		//Remove from queue, add to balance
		CurrentPassenger = PassengerQueue.Dequeue();
		Balance += CurrentPassenger.FareToPay;
		if (debug) Debug.Log($"[FareManager] Received payment: P{CurrentPassenger.FareToPay}");

		//Mark as fully paid if no change
		if (CurrentPassenger.FareOwed >= CurrentPassenger.FareToPay) {
			CurrentPassenger.FullyPaid = true;
			if (debug) Debug.Log($"[FareManager] Complete transaction.");

			CurrentPassenger = null;
		}

		//Calculate the expected change
		else {
			CurrentPassenger.ExpectedChange = CurrentPassenger.FareToPay - CurrentPassenger.FareOwed;
			if (debug) Debug.Log($"[FareManager] Incomplete transaction. Expected change: P{CurrentPassenger.ExpectedChange}.");
		}

		RearviewSprite.color = Color.white;
	}

	#endregion

	#region Change

	/// <summary>
	/// The amount of change that is currently staged to be given to the passenger.
	/// </summary>
	[Header("Change")]
	public int StagedChange = 0;

	/// <summary>
	/// The denominations of coins that are currently staged to be given to the passenger.
	/// </summary>
	public List<int> StagedCoins = new();

	/// <summary>
	/// Adds the specified amount to the staged change.
	/// </summary>
	/// <param name="amount">How much to add to the staged change.</param>
	public void StageChange(int amount, bool debug = false) {
		if (!Denominations.Contains(amount)) {
			Debug.LogError($"[FareManager] Invalid cash value: {amount}. Must be one of the denominations.");
			return;
		}

		StagedCoins.Add(amount);
		StagedChange += amount;

		if (debug) Debug.Log($"[FareManager] Staged change: P{amount}, Total: P{StagedChange}");
	}

	/// <summary>
	/// Removes the specified amount from the staged change if it exists.
	/// </summary>
	/// <param name="amount">Amount to unstage.</param>
	/// <param name="debug">Whether to print logs to console.</param>
	public void UnstageChange(int amount, bool debug = false) {
		if (StagedCoins.Contains(amount)) {
			StagedCoins.Remove(amount);
			StagedChange -= amount;
		}

		else {
			if (debug) Debug.Log($"[FareManager] Cannot unstage change of P{amount} as it is not staged.");
		}
	}

	/// <summary>
	/// Gives the change to the current passenger.
	/// </summary>
	/// <param name="debug">Whether to print logs to console.</param>
	public void GiveChange(bool debug = false) {
		if (CurrentPassenger == null) {
			Debug.LogError("[FareManager] Cannot give change to a null passenger. Was CurrentPassenger assigned properly?");
			return;
		}

		if (CurrentPassenger.ExpectedChange <= 0) {
			Debug.LogError("[FareManager] Cannot give change to passenger with no expected change. This is not normal behavior. Please check calculation logic.");
			return;
		}

		if (StagedChange <= 0 && StagedCoins.Count == 0) {
			Debug.LogError("[FareManager] Cannot give change to passenger with no staged change or coins. This is not normal behavior. Please check calculation logic.");
			return;
		}

		Balance = Math.Max(0, Balance - StagedChange);

		CurrentPassenger.ReceiveChange(StagedChange, debug);

		if (debug) Debug.Log($"[FareManager] Gave change to passenger: P{StagedChange} / P{CurrentPassenger.ExpectedChange}.");

		CurrentPassenger = null;
	}

	#endregion

	#region Commit Change

	/// <summary>
	/// Scans for mouse clicks to commit or clear staged change.
	/// </summary>
	/// <param name="debug">Whether to print logs to console.</param>
	private void ScanMouseClick(bool debug = false) {
		//Clear staged coins and give change
		if (InputManager.OnMouse1) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if (Collider.OverlapPoint(mousePosition)) {
				if (debug) Debug.Log("[FareManager] Mouse click detected on accept button.");

				GiveChange(debug);
				StagedChange = 0;
				StagedCoins.Clear();
			}
		}

		//Clear staged coins
		else if (InputManager.OnMouse2) {
			if (StagedChange > 0 || StagedCoins.Count > 0) {
				StagedChange = 0;
				StagedCoins.Clear();
			}
		}
	}

	/// <summary>
	/// The box collider where the player can click to commit/cancel staged change.
	/// </summary>
	[Header("Game Objects & Components")]
	public BoxCollider2D Collider;

	/// <summary>
	/// The input manager that handles mouse clicks and other inputs.
	/// </summary>
	public InputManager InputManager;

	#endregion

	#region Penalty

	public int TotalPenalty = 0;

	public int NetBalance = 0;

	public int CollisionPenalty = 100;

	public int NoPayPenalty = 300;

	public int SlowChangePenalty = 300;

	public int MissedStopPenalty = 50;

	public enum PenaltyType {
		None,
		Collision,
		NoPay,
		SlowChange
	}

	public void Penalize(PenaltyType penaltyType, bool debug = false) {
		switch (penaltyType) {
			case PenaltyType.None:
				if (debug) Debug.Log("[Passenger] No penalty applied.");
				break;
			case PenaltyType.Collision:
				TotalPenalty += CollisionPenalty;
				if (debug) Debug.Log($"[Passenger] Collision penalty: P{TotalPenalty} +P{CollisionPenalty}");
				break;
			case PenaltyType.NoPay:
				TotalPenalty += NoPayPenalty;
				if (debug) Debug.Log($"[Passenger] No pay penalty: P{TotalPenalty} +P{NoPayPenalty}");
				break;
			case PenaltyType.SlowChange:
				TotalPenalty += SlowChangePenalty;
				if (debug) Debug.Log($"[Passenger] Slow change penalty: P{TotalPenalty} +P{SlowChangePenalty}");
				break;
		}

		NetBalance = Balance - TotalPenalty;
	}

	#endregion

	#region Unity Callbacks

	private void Update() {
		ScanMouseClick(true);

		UpdateDisplay();
	}

	public TextMeshProUGUI DisplayText;

	private void UpdateDisplay() {
		if (CurrentPassenger == null) {
			DisplayText.text = "";
			return;
		}

		int payment = CurrentPassenger.FareToPay;
		int owed = CurrentPassenger.FareOwed;
		int change = StagedChange;
		string text = $"Paid: {payment}\nOwed: {owed}\nChange: {change}";

		DisplayText.text = text;
	}

	#endregion

	#region Static Members

	public static int[] Denominations = new int[] { 1, 5, 10, 20, 50, 100, 200, 500, 1000 };

	/// <summary>
	/// Divides the value into an array of coins that sum up to the value.
	/// </summary>
	/// <param name="value">The value to divide into coins.</param>
	/// <returns>A heap of coins.</returns>
	public static int[] RoundToCoins(int value, bool debug = false) {
		if (value < 0) {
			Debug.LogError("[FareManager] Cannot round negative values to coins.");
			return Array.Empty<int>();
		}

		List<int> coins = new();

		//Starts at P20. P50+ don't have coins.
		for (int i = 3; i >= 0; i--) {
			int denomination = Denominations[i];

			while (value >= denomination) {
				coins.Add(denomination);
				value -= denomination;

				if (debug) Debug.Log($"[FareManager] Added coin: P{denomination}");
			}
		}

		return coins.ToArray();
	}

	/// <summary>
	/// Rounds a value to the nearest bill denomination.
	/// <br/><br/>
	/// This is normally used to round the fare owed by a passenger to the
	/// nearest bill denomination to avoid paying in strangely specific amounts.
	/// </summary>
	/// <param name="value">The value to round.</param>
	/// <returns></returns>
	public static int RoundToBill(int value, bool roundCoins = false, bool debug = false) {
		
		if (value < 50 && !roundCoins) {
			return value;
		}

		for (int i = Denominations.Length - 1; i >= 0; i--) {
			if (value >= Denominations[i]) {
				if (debug) Debug.Log($"[FareManager] Rounded {value} to P{Denominations[i]}.");
				return Denominations[i];
			}
		}

		Debug.LogError($"[FareManager] No suitable denomination found for value {value}.");
		return -1;
	}

	/// <summary>
	/// Calculates a biased random value between min and max, biased towards the average.
	/// </summary>
	/// <param name="min">The minimum value to return.</param>
	/// <param name="max">The maximum value to return.</param>
	/// <param name="average">The value to skew towards.</param>
	/// <returns></returns>
	public static double BiasedRandom(double min, double max, double average) {
		Random random = new();

		double range = max - min;
		double midpoint = (min + max) / 2;

		//Whether the average is above or below the midpoint
		bool biasRight = average > midpoint;

		//Bias strength is determined by how far the average is from the midpoint
		double distance = Math.Abs(average - midpoint) / (range / 2);
		double exponent = biasRight ? (1.0 - distance) * 2 + 1 : (1.0 + distance) * 2 + 1;

		//Skew the random value
		double random01 = random.NextDouble();
		double skewed = biasRight ? Math.Pow(random01, 1.0 / exponent) : 1 - Math.Pow(1 - random01, 1.0 / exponent);

		return min + skewed * range;
	}

	#endregion
}
