using System.Linq;
using UnityEngine;

public class CashStash : MonoBehaviour
{
	public int CashValue = 0;

	public FareManager FareManager;

	public BoxCollider2D Collider;

	public InputManager InputManager;

	/// <summary>
	/// Scans for mouse clicks to commit or clear staged change.
	/// </summary>
	/// <param name="debug">Whether to print logs to console.</param>
	private void ScanMouseClick(bool debug = false) {

		//Clear staged coins and give change
		if (InputManager.ClickedLeft) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if (Collider.OverlapPoint(mousePosition)) {
				FareManager.StageChange(CashValue);

				if (debug) Debug.Log($"[CashStash] Staged P{CashValue}. Total: {FareManager.StagedCoins.Count}.");
			}
		}

		//Clear staged coins
		else if (InputManager.ClickedRight) {
			if (Collider.OverlapPoint(Input.mousePosition)) {
				FareManager.UnstageChange(CashValue);

				if (debug) Debug.Log($"[CashStash] Unstaged P{CashValue}. Total: {FareManager.StagedCoins.Count}.");
			}
		}
	}

	#region Unity Callbacks

	private void Start() {
		if (!FareManager.Denominations.Contains(CashValue)) {
			Debug.LogError($"[CashStash] Invalid cash value: {CashValue}. Must be one of the denominations.");
			return;
		}
	}

	private void Update() {
		ScanMouseClick();
	}

	#endregion
}
