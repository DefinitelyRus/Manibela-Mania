using UnityEngine;

public class CashStash : MonoBehaviour
{
	public int CoinValue = 0;

	public FareManager FareManager;

	public BoxCollider2D Collider;

	public InputManager InputManager;

	private void Start() {
		// Validate that the coin value is one of the allowed denominations
		bool isValidDenomination = false;
		foreach (int denomination in FareManager.Denominations) {
			if (CoinValue == denomination) {
				isValidDenomination = true;
				break;
			}
		}

		if (!isValidDenomination) {
			Debug.LogError($"[CashStash] Invalid coin value: {CoinValue}. Must be one of the denominations: {string.Join(", ", FareManager.Denominations)}");
		}
	}

	private void Update() {
		if (InputManager.OnMouse1) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if (Collider.OverlapPoint(mousePosition)) {
				if (FareManager.CurrentPassenger != null) {
					// Add the coin value to the current passenger's payment
					FareManager.CurrentPassenger.FareToPay += CoinValue;
					Debug.Log($"[CashStash] Added P{CoinValue} to payment. Total: P{FareManager.CurrentPassenger.FareToPay}");
				}
			}
		}
	}
}
