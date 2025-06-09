using UnityEngine;
using System.Collections.Generic;

public class FareManager : MonoBehaviour {
	[Header("Money")]
	public int Balance = 0;
	public int TotalPenalty = 0;
	public int NetBalance = 0;

	// Simplified to just 4 coin denominations
	public static int[] Denominations = new int[] { 1, 5, 10, 20 };

	[Header("Game Objects & Components")]
	public BoxCollider2D Collider;
	public InputManager InputManager;

	// Simplified passenger queue
	public Queue<BoardedPassenger> PassengerQueue { get; private set; } = new();
	public BoardedPassenger CurrentPassenger { get; private set; }

	public void QueuePayment(BoardedPassenger passenger, bool debug = false) {
		if (passenger == null) {
			Debug.LogError("[FareManager] Cannot queue payment for a null passenger.");
			return;
		}

		PassengerQueue.Enqueue(passenger);
		if (debug) Debug.Log($"[FareManager] Queued passenger for payment.");
	}

	public void AcceptPayment(bool debug = false) {
		if (PassengerQueue.Count == 0) {
			Debug.LogError("[FareManager] No passengers in queue to accept payment from.");
			return;
		}

		CurrentPassenger = PassengerQueue.Dequeue();
		Balance += CurrentPassenger.FareToPay;
		
		// If the payment covers or exceeds the fare, mark as paid and ready for drop-off
		if (CurrentPassenger.FareToPay >= CurrentPassenger.FareOwed) {
			CurrentPassenger.FullyPaid = true;
			CurrentPassenger.ToDropOff = true;
			if (debug) Debug.Log($"[FareManager] Payment accepted. Fare: P{CurrentPassenger.FareOwed}, Paid: P{CurrentPassenger.FareToPay}");
		}
		
		CurrentPassenger = null;
	}

	private void Update() {
		if (InputManager.OnMouse1) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if (Collider.OverlapPoint(mousePosition)) {
				AcceptPayment(true);
			}
		}
	}
}
