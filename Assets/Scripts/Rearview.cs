using UnityEngine;

public class Rearview : MonoBehaviour
{
	public InputManager Inputs;

	public FareManager Fares;

	public BoxCollider2D Collider;

    void Update()
    {
		//Check if mouse is over this object
		if (Inputs.OnMouse1) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if (Collider.OverlapPoint(mousePosition)) {
				Debug.Log("[Rearview] Mouse click detected on rearview mirror.");

				if (Fares.PassengerQueue.Count == 0) {
					Debug.LogWarning("[Rearview] No passenger to accept payment from.");
					return;
				}

				Fares.AcceptPayment(true);

				//TODO: Sprite updates here
			}
		}
	}
}
