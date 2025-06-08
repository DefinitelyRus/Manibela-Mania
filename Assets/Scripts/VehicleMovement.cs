using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the movement, braking, steering, and gear shifting of a 2D vehicle. <br/>
/// Attach this to a vehicle GameObject with a Rigidbody2D.
/// <br/><br/>
/// Required setup: <br/>
/// 1. Attach this script to your vehicle GameObject. <br/>
/// 2. Assign a Rigidbody2D to the "Body" field. <br/>
/// 3. Assign an InputManager instance to the "Input" field. <br/>
/// 4. (Optional) Tune the parameters to your liking in the Unity Inspector. <br/>
/// </summary>
public class VehicleMovement : MonoBehaviour {

	#region Forward Movement

	#region Speed & Acceleration

	/// <summary>
	/// How many units/sec the vehicle is currently traversing.
	/// </summary>
	[Header("Speed & Acceleration")]
	public float Speed = 0f;

	/// <summary>
	/// The absolute maximum speed the vehicle can
	/// move in regardless of direction or gear.
	/// </summary>
	public float MaxSpeed = 200f;

	/// <summary>
	/// Accelerates the vehicle based on the current gear and input.
	/// </summary>
	/// <param name="debug"></param>
	private void Accelerate(bool debug = false) {
		float accelerationTime = Time.deltaTime; // Use delta time directly for simplicity
		float speedIncrement = 0f;

		bool inGear2Range = Speed >= Gear1Speed * ManualShiftUpThreshold && Speed < Gear2Speed;
		bool inGear3Range = Speed >= Gear2Speed * ManualShiftUpThreshold && Speed < Gear3Speed;

		//Increase the speed based on the current gear and acceleration time
		if (CurrentGear == 1)
			speedIncrement = Gear1Speed * accelerationTime / Gear1Time;

		else if (CurrentGear == 2 && inGear2Range)
			speedIncrement = Gear2Speed * accelerationTime / Gear2Time;

		else if (CurrentGear == 3 && inGear3Range)
			speedIncrement = Gear3Speed * accelerationTime / Gear3Time;

		else if (CurrentGear == -1) speedIncrement = -GearRSpeed * accelerationTime / GearRTime;

		Speed += speedIncrement;

		//Clamp the speed based on current gear
		if (CurrentGear == 1) {
			Speed = Mathf.Clamp(Speed, 0, Gear1Speed);
			AudioHandler.SetLooping(VehicleSFX.LoopingSFX.Gear1);
		}
		else if (CurrentGear == 2) {
			Speed = Mathf.Clamp(Speed, 0, Gear2Speed);
			AudioHandler.SetLooping(VehicleSFX.LoopingSFX.Gear2);
		}
		else if (CurrentGear == 3) {
			Speed = Mathf.Clamp(Speed, 0, Gear3Speed);
			AudioHandler.SetLooping(VehicleSFX.LoopingSFX.Gear3);
		}
		else if (CurrentGear == -1) {
			Speed = Mathf.Clamp(Speed, -GearRSpeed, 0);
			AudioHandler.SetLooping(VehicleSFX.LoopingSFX.Idle);
		}
		else if (CurrentGear == 0) {
			AudioHandler.SetLooping(VehicleSFX.LoopingSFX.Idle);
		}
	}

	#endregion

	#region Gears

	/// <summary>
	/// Controls the speed at which the vehicle can move when accelerating.
	/// </summary>
	[Header("Gears")]
	public int CurrentGear = 0;

	/// <summary>
	/// The amount of speed to boost when shifting up a gear.
	/// <br/><br/>
	/// This is mostly used to prevent the player from immediately
	/// auto-shifting down or stalling when shifting up a gear,
	/// as it gives the player a slight speed boost.<br/>
	/// It also helps make the initial push when shifting
	/// up a gear feel more powerful.
	/// </summary>
	public float ShiftUpBoost = 0.1f;

	/// <summary>
	/// Whether the player can shift gears automatically or not.
	/// </summary>
	public bool UseAutoShift = false;

	/// <summary>
	/// The speed percentage at which the tachometer will
	/// shift up gears when the player manually shifts up.
	/// </summary>
	public float ManualShiftUpThreshold = 0.5f;

	/// <summary>
	/// The speed percentage at which the tachometer will automatically shift up gears.
	/// </summary>
	public float AutoShiftUpThreshold = 0.95f;

	/// <summary>
	/// The speed percentage at which the tachometer will automatically shift down gears.
	/// </summary>
	public float AutoShiftDownThreshold = 0.05f;

	/// <summary>
	/// How close the player is to the gear's maximum speed.
	/// </summary>
	public float Tachometer = 0f;

	/// <summary>
	/// Updates the tachometer based on the current speed and gear.
	/// </summary>
	private void UpdateTachometer() {
		Tachometer = CurrentGear switch {
			1 => Speed / Gear1Speed,
			2 => (Speed - Gear1Speed) / (Gear2Speed - Gear1Speed),
			3 => (Speed - Gear2Speed) / (Gear3Speed - Gear2Speed),
			-1 => -Speed / GearRSpeed,
			_ => 0f,
		};
	}

	/// <summary>
	/// Changes the current gear to a specific gear.
	/// </summary>
	/// <param name="shiftTo">Move up or down to a specific gear.</param>
	/// <param name="debug">Whether to print logs to console.</param>
	public void ChangeGear(int shiftTo, bool debug = false) {
		if (shiftTo < -1 || shiftTo > 3) {
			Debug.LogError($"[PlayerMovement] Invalid gear change. Attempting to shift to gear {shiftTo}.");
			return;
		}

		if (CurrentGear == shiftTo) {
			if (debug) Debug.Log($"[PlayerMovement] Already in gear {CurrentGear}.");
			return;
		}

		if (shiftTo == 1 && (Speed > Gear1Speed || Speed < 0)) {
			if (debug) Debug.Log($"[PlayerMovement] Cannot shift to 1st gear while speed is above {Gear1Speed:F0}.");
			return;
		}

		else if (shiftTo == 2 && (Speed > Gear2Speed || Speed < 0)) {
			if (debug) Debug.Log($"[PlayerMovement] Cannot shift to 2nd gear while speed is above {Gear2Speed:F0}.");
			return;
		}

		else if (shiftTo == 3 && (Speed > Gear3Speed || Speed < 0)) {
			if (debug) Debug.Log($"[PlayerMovement] Cannot shift to 3rd gear while speed is above {Gear3Speed:F0}.");
			return;
		}

		else if (shiftTo == -1 && Speed > 0) {
			if (debug) Debug.Log("[PlayerMovement] Cannot shift to reverse gear while speed is positive.");
			return;
		}

		//Add boost
		switch (shiftTo) {
			case 0:
				AudioHandler.SetLooping(VehicleSFX.LoopingSFX.Idle);
				break;
			case 1:
				Speed *= 1 + ShiftUpBoost;
				Mathf.Clamp(Speed, 0, Gear1Speed);
				if (Speed > MaxSpeed) Speed = MaxSpeed;
				break;
			case 2:
				Speed *= 1 + ShiftUpBoost;
				if (Speed > MaxSpeed) Speed = MaxSpeed;
				break;
			case 3:
				Speed *= 1 + ShiftUpBoost;
				if (Speed > MaxSpeed) Speed = MaxSpeed;
				break;
		}

		CurrentGear = shiftTo;

		//Play audio
		AudioHandler.Play(VehicleSFX.OneShotSFX.GearShift);

		if (debug) Debug.Log($"[PlayerMovement] Changed gear to {CurrentGear}.");
	}

	/// <summary>
	/// Changes the current gear by the specified amount.
	/// </summary>
	/// <param name="shiftBy">Move up or down by this many gears.</param>
	/// <param name="debug">Whether to print logs to console.</param>
	public void ShiftGear(int shiftBy, bool debug = false) {
		ChangeGear(CurrentGear + shiftBy, debug);
	}

	/// <summary>
	/// Automatically shifts gears based on the current speed and input.
	/// </summary>
	/// <param name="useAutoShift"></param>
	private void AutoShift(bool useAutoShift = false, bool debug = false) {
		if (!useAutoShift) return;

		bool isStopping = CurrentGear > 0 && Mathf.Abs(Speed) < 500 && !Input.IsAccelerating;
		bool acceleratingAtStop = CurrentGear == 0 && Input.IsAccelerating;
		bool toGear1Up = Speed < Gear1Speed * AutoShiftDownThreshold;
		bool inGear1Range = toGear1Up && Mathf.Abs(Speed) > 500;

		bool toGear2Up = Speed > Gear1Speed * AutoShiftUpThreshold;
		bool toGear2Down = Speed < Gear2Speed * AutoShiftDownThreshold;
		bool inGear2Range = toGear2Up && toGear2Down;

		bool toGear3Up = Speed > Gear2Speed * AutoShiftUpThreshold;
		bool toGear3Down = Speed < Gear3Speed * AutoShiftDownThreshold;
		bool inGear3Range = toGear3Up && toGear3Down;

		if (isStopping) {
			ChangeGear(0, debug);
			return;
		}

		if (inGear1Range || acceleratingAtStop) {
			ChangeGear(1, debug);
			return;
		}

		//Gear 2
		if (inGear2Range) {
			ChangeGear(2, debug);
			return;
		}

		//Gear 3
		if (inGear3Range) {
			ChangeGear(3, debug);
			return;
		}
	}

	#region Gear Speeds & Times

	/// <summary>
	/// How many units/sec the vehicle can traverse while in 1st gear.
	/// </summary>
	[Header("Gear Speeds & Times")]
	public float Gear1Speed = 30f;

	/// <summary>
	/// How many units/sec the vehicle can traverse while in 2nd gear.
	/// </summary>
	public float Gear2Speed = 80f;

	/// <summary>
	/// How many units/sec the vehicle can traverse while in 3rd gear.
	/// </summary>
	public float Gear3Speed = 120f;

	/// <summary>
	/// The maximum units/sec the vehicle can traverse while in reverse.
	/// </summary>
	public float GearRSpeed = 40f;

	/// <summary>
	/// How many seconds it takes to reach <see cref="Gear1Speed"/>.
	/// </summary>
	public float Gear1Time = 4f;

	/// <summary>
	/// How many seconds it takes to reach <see cref="Gear2Speed"/>.
	/// </summary>
	public float Gear2Time = 8f;

	/// <summary>
	/// How many seconds it takes to reach <see cref="Gear3Speed"/>.
	/// </summary>
	public float Gear3Time = 16f;

	/// <summary>
	/// How many seconds it takes to reach <see cref="GearRSpeed"/>.
	/// </summary>
	public float GearRTime = 2f;

	#endregion

	#endregion

	#region Braking

	/// <summary>
	/// How many seconds it takes for <see cref="BrakeDecel"/> to reach <see cref="BrakeMaxDecel"/>.
	/// </summary>
	[Header("Braking & Deceleration")]
	public float BrakeMaxTime = 2f;

	/// <summary>
	/// How many seconds the vehicle has been braking.
	/// </summary>
	private float BrakeTime = 0f;

	/// <summary>
	/// How much to reduce <see cref="Speed"/> by every second when braking.
	/// </summary>
	public float BrakeDecel = 0f;

	/// <summary>
	/// The maximum of how much to reduce <see cref="Speed"/> by every second when braking.
	/// </summary>
	public float BrakeMaxDecel = 80f;

	/// <summary>
	/// How much to reduce <see cref="Speed"/> by every second when handbraking.
	/// </summary>
	public float HandbrakeDecel = 150f;

	/// <summary>
	/// The percentage of the current gear's speed that the vehicle
	/// will naturally decelerate to when not accelerating.
	/// </summary>
	public float NaturalDecel = 0.1f;

	/// <summary>
	/// Gradually reduces the vehicle's speed based on the current gear and natural deceleration.
	/// </summary>
	private void NaturallyDecelerate() {
		float speedDecrement = 0f;

		if (CurrentGear == 0 || CurrentGear == 1) {
			speedDecrement -= Gear1Speed * NaturalDecel * Time.deltaTime;

			if (Speed < 0) {
				speedDecrement = 0;
				Speed = 0;
			}
		}

		else if (CurrentGear == 2) {
			speedDecrement -= Gear2Speed * NaturalDecel * Time.deltaTime;

			if (Speed < 0) {
				speedDecrement = 0;
				Speed = 0;
			}
		}

		else if (CurrentGear == 3) {
			speedDecrement -= Gear3Speed * NaturalDecel * Time.deltaTime;

			if (Speed < 0) {
				speedDecrement = 0;
				Speed = 0;
			}
		}

		else if (CurrentGear == -1) {
			speedDecrement -= GearRSpeed * NaturalDecel * Time.deltaTime;

			if (Speed > 0) {
				speedDecrement = 0;
				Speed = 0;
			}
		}

		if (Mathf.Abs(Speed) < 0.1f) {
			Speed = 0;
		}
		else Speed += speedDecrement;
	}

	/// <summary>
	/// Applies the foot brake to the vehicle, gradually reducing speed.
	/// </summary>
	private void FootBrake() {
		BrakeTime = Mathf.Clamp(BrakeTime + Time.deltaTime, 0, BrakeMaxTime);

		BrakeDecel = Mathf.Lerp(0, BrakeMaxDecel, BrakeTime / BrakeMaxTime);

		Speed -= BrakeDecel * Time.deltaTime;

		if (Speed < 0) {
			Speed = 0; // Prevent negative speed
			BrakeTime = 0; // Reset brake time when speed reaches zero
		}
	}

	/// <summary>
	/// Applies the hand brake to the vehicle, immediately reducing speed.
	/// </summary>
	private void HandBrake() {
		Speed -= HandbrakeDecel * Time.deltaTime;

		if (Speed < 0) {
			Speed = 0;
		}
	}

	#endregion

	#endregion

	#region Steering

	/// <summary>
	/// The maximum degrees the vehicle will turn.
	/// </summary>
	public float SteeringSensitivity = 25f;

	/// <summary>
	/// How fast the vehicle will reach its maximum turning angle.
	/// <br/><br/>
	/// This is measured in seconds but only the denominator.
	/// For example, in 0.5s = 1/2, you only use the 2.
	/// </summary>
	public float SteerSpeed = 2f;

	/// <summary>
	/// The target angle the vehicle is trying to reach based on the input.
	/// </summary>
	private float TargetAngle = 0f;

	/// <summary>
	/// Steers the vehicle towards the direction of the input over the span of 0.5 seconds.
	/// <br/><br/>
	/// Not to be confused see <see cref="SteerPhysics"/>, which is called in <see cref="FixedUpdate"/>.
	/// </summary>
	public void Steer() {
		TargetAngle = Mathf.LerpAngle(0, SteeringSensitivity * Mathf.Sign(Input.Steering), Mathf.Abs(Input.Steering));

	}

	/// <summary>
	/// Applies the steering physics to the vehicle, smoothly transitioning to the target angle.
	/// <br/><br/>
	/// Unlike <see cref="Steer"/>, this method is called in
	/// <see cref="FixedUpdate"/> to ensure consistent physics updates.
	/// </summary>
	private void SteerPhysics() {
		float currentAngle = transform.eulerAngles.z;
		float newAngle = Mathf.LerpAngle(currentAngle, TargetAngle, Time.fixedDeltaTime * SteerSpeed);

		transform.rotation = Quaternion.Euler(0, 0, newAngle);

	}

	#endregion

	#region Camera Shake & Collision

	/// <summary>
	/// Handles camera shake and collision effects when the vehicle collides with something.
	/// </summary>
	[Header("Camera Shake & Collision")]
	public CameraHandler Camera;

	/// <summary>
	/// The magnitude of the camera shake when a collision occurs.
	/// </summary>
	public float CollisionShakeMagnitude = 3f;

	/// <summary>
	/// The speed reduction factor applied to the vehicle's speed when a collision occurs.
	/// </summary>
	public float CollisionSpeedReduction = 0.5f;

	private void OnCollisionEnter2D(Collision2D collision) {

		bool cameraExists = Camera != null;
		bool significantCollision = collision.relativeVelocity.magnitude > 3f;
		if (cameraExists && significantCollision) Camera.TriggerShake();

		Speed *= CollisionSpeedReduction;
	}

	#endregion

	#region Game Objects & Components

	[Header("Game Objects & Components")]
	public InputManager Input;

	public Rigidbody2D Body;

	public VehicleSFX AudioHandler;

	#endregion

	#region Unity Callbacks

	private void Update() {

		#region Inputs

		if (Input.IsDecelerating) FootBrake();
		else if (Input.IsHandBraking) HandBrake();
		else if (Input.IsAccelerating) Accelerate();
		else {
			NaturallyDecelerate();
			BrakeTime = 0;
		}

		if (!Input.IsDecelerating && Mathf.Abs(Speed) < 0.1f) {
			BrakeTime = 0;
			BrakeDecel = 0;
		}

		//Inputs are handled in the method itself.
		Steer();

		if (Input.OnGearUp) ShiftGear(1, true);
		if (Input.OnGearDown) ShiftGear(-1, true);

		if (Input.OnGear1) ChangeGear(1, true);
		if (Input.OnGear2) ChangeGear(2, true);
		if (Input.OnGear3) ChangeGear(3, true);
		if (Input.OnGearR) ChangeGear(-1, true);

		if (Input.OnHandBrake) AudioHandler.Play(VehicleSFX.OneShotSFX.Handbrake);

		if (Input.OnHonk) {
			//TODO: Honk logic here?
			AudioHandler.Play(VehicleSFX.OneShotSFX.Horn);
		}
		#endregion

		AutoShift(UseAutoShift);

		UpdateTachometer();
	}

	private void FixedUpdate() {
		if (Speed != 0) Body.linearVelocity = Speed * Time.fixedDeltaTime * transform.up;
		else Body.linearVelocity = Vector2.zero;

		// Project current velocity onto the forward vector only
		float forwardSpeed = Vector2.Dot(Body.linearVelocity, transform.up);
		Body.linearVelocity = transform.up * forwardSpeed;

		SteerPhysics();
	}

	#endregion
}
