using System;
using UnityEngine;

public class Autopilot : InputManager
{
	[Header("Autopilot Settings")]
	public float ScanRange = 300f;

	public float ScanRangeCritical = 100f;

	public Transform CenterScanner;

	public Transform LeftScanner;

	public Transform RightScanner;

	public float ChangeLaneDistance = 20f;

	private float CurrentLaneX = 0f;

	private float TargetLaneX = 0f;

	public LayerMask Obstacles;

	public VehicleMovement Movement;

	public void ScanFront(bool debug = false) {

		bool critCenter = Physics2D.Raycast(CenterScanner.position, new(0, 1), ScanRangeCritical, Obstacles);
		bool warnCenter = Physics2D.Raycast(CenterScanner.position, new(0, 1), ScanRange, Obstacles);
		bool warnLeft = Physics2D.Raycast(LeftScanner.position, new(0, 1), ScanRange, Obstacles);
		bool warnRight = Physics2D.Raycast(RightScanner.position, new(0, 1), ScanRange, Obstacles);

		if (debug) {
			Debug.DrawRay(CenterScanner.position, new Vector3(0, 1) * ScanRange, Color.yellow);
			Debug.DrawRay(CenterScanner.position, new Vector3(0, 1) * ScanRangeCritical, Color.red);
			Debug.DrawRay(LeftScanner.position, new Vector3(0, 1) * ScanRange, Color.yellow);
			Debug.DrawRay(RightScanner.position, new Vector3(0, 1) * ScanRange, Color.yellow);
		}

		if (warnCenter) {
			IsAccelerating = false;

			//If the left lane is clear, turn left
			if (!warnLeft) {
				TargetLaneX = CurrentLaneX - ChangeLaneDistance;
				Steering = 1;

				if (debug) Debug.Log("[Autopilot.ScanFront] Turning Left to avoid obstacle.");
			}

			//If the right lane is clear, turn right
			else if (!warnRight) {
				TargetLaneX = CurrentLaneX + ChangeLaneDistance;
				Steering = -1;

				if (debug) Debug.Log("[Autopilot.ScanFront] Turning Right to avoid obstacle.");
			}

			//If both lanes are not clear, keep straight and brake.
			else if (warnLeft && warnRight) {
				Steering = 0;
				Debug.Log($"[Autopilot.ScanFront] Both lanes blocked, keeping straight and stopping.");

				if (critCenter) {
					HardBrake();

					if (debug) Debug.Log("[Autopilot.ScanFront] Close obstacle detected, hard braking!");
				}

				else if (Movement.Speed > Movement.Gear1Speed) {
					SoftBrake();

					if (debug) Debug.Log("[Autopilot.ScanFront] Distant obstacle detected, soft braking!");
				}
			}
		}

		else {
			IsAccelerating = true;

			if (debug) Debug.Log("[Autopilot.ScanFront] No obstacles detected, continuing straight.");
		}

		if (debug) Debug.Log($"[Autopilot.ScanFront] PosX: {transform.position.x:F0}, Current Lane: {CurrentLaneX:F0}, Target Lane: {TargetLaneX:F0}, Steering: {Steering:F0}");

		Steer(debug);
	}

	public void Steer(bool debug = false) {
		if (Steering == 0) {
			if (debug) Debug.Log("[Autopilot.Steer] No steering action required.");
			return;
		}

		float posX = transform.position.x;

		//Turning left
		if (Steering == 1) {
			if (posX < TargetLaneX) {
				CurrentLaneX = posX;
				Steering = 0;

				if (debug) Debug.Log($"[Autopilot.Steer] Completed lane change to left.");
				return;
			}

			else if (debug) Debug.Log($"[Autopilot.Steer] Steering Left to lane at X: {TargetLaneX}");
		}

		//Turning right
		if (Steering == -1) {
			if (posX > TargetLaneX) {
				CurrentLaneX = posX;
				Steering = 0;

				if (debug) Debug.Log($"[Autopilot.Steer] Completed lane change to right.");
				return;
			}

			else if (debug) Debug.Log($"[Autopilot.Steer] Steering Right to lane at X: {TargetLaneX}");
		}
	}

	public void SoftBrake() {
		IsDecelerating = true;
		IsAccelerating = false;
	}

	public void HardBrake() {
		IsDecelerating = false;
		IsAccelerating = false;
		IsHandBraking = true;
	}

	private bool HasHonked = false;

	public void UseHonk() {

	}

	public override void InputListener() {

	}

	public void Start() {
		CurrentLaneX = transform.position.x;

		int startingGear = UnityEngine.Random.Range(1, 4);

		if (startingGear == 1) Movement.ChangeGear(startingGear);
		else if (startingGear == 2) {
			Movement.Speed = Movement.Gear1Speed;
			Movement.ChangeGear(startingGear);
		}
		else if (startingGear == 3) {
			Movement.Speed = Movement.Gear2Speed;
			Movement.ChangeGear(startingGear);
		}
	}

	public void Update() {

		OnGear1 = false;
		OnGear2 = false;
		OnGear3 = false;
		InputListener();

		ScanFront();
	}
}
