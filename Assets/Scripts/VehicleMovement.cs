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


	[SerializeField]private SoundData idleSound;
	[SerializeField]private SoundData[] gearSounds = new SoundData[3];
	[SerializeField] private SoundData brakeSound;
	[SerializeField] private SoundData crashSound;
	[SerializeField]private AudioSource engineSource;

	[SerializeField] private SoundData gearShiftSound;
	
	[SerializeField] private AudioSource sfxSource;
	private int previousGear;

	public CameraHandler camHandler;

	#region Forward Movement

	#region Speed & Acceleration

	/// <summary>
	/// How many units/sec the vehicle is currently traversing.
	/// </summary>
	[Header("Speed & Acceleration")]
	public float Speed = 0f;

	/// <summary>
	/// The absolute maximum speed the vehicle can move in regardless of direction or gear.
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


		if (CurrentGear < previousGear)
		{
			PlayOneShotSound(gearShiftSound, 0.9f); // Downshift
			pitchOverrideValue = 0.8f;
			pitchOverrideTimer = 0.3f;
		}
		else if (CurrentGear > previousGear)
		{
			PlayOneShotSound(gearShiftSound, 1.1f); // Upshift
		}

	previousGear = CurrentGear;

		//Clamp the speed based on current gear
		if (CurrentGear == 1) Speed = Mathf.Clamp(Speed, 0, Gear1Speed);
		else if (CurrentGear == 2) Speed = Mathf.Clamp(Speed, 0, Gear2Speed);
		else if (CurrentGear == 3) Speed = Mathf.Clamp(Speed, 0, Gear3Speed);
		else if (CurrentGear == -1) Speed = Mathf.Clamp(Speed, -GearRSpeed, 0);
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
	/// This is mostly used to prevent the player from immediately auto-shifting down or stalling
	/// when shifting up a gear, as it gives the player a slight speed boost.<br/>
	/// It also helps make the initial push when shifting up a gear feel more powerful.
	/// </summary>
	public float ShiftUpBoost = 0.1f;

	/// <summary>
	/// Whether the player can shift gears automatically or not.
	/// </summary>
	public bool UseAutoShift = false;

	/// <summary>
	/// The speed percentage at which the tachometer will shift up gears when the player manually shifts up.
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
	private void ChangeGear(int shiftTo, bool debug = false) {
		if (shiftTo < -1 || shiftTo > 3) {
			Debug.LogError($"[PlayerMovement] Invalid gear change. Attempting to shift to gear {shiftTo}.");
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

		CurrentGear = shiftTo;

		//Apply speed boost on gear change.
		if (shiftTo == 1) {
			Speed *= 1 + ShiftUpBoost;
			if (Speed > MaxSpeed) Speed = MaxSpeed;
		}

		if (debug) Debug.Log($"[PlayerMovement] Changed gear to {CurrentGear}.");
	}

	/// <summary>
	/// Changes the current gear by the specified amount.
	/// </summary>
	/// <param name="shiftBy">Move up or down by this many gears.</param>
	/// <param name="debug">Whether to print logs to console.</param>
	private void ShiftGear(int shiftBy, bool debug = false) {
		ChangeGear(CurrentGear + shiftBy, debug);
	}

	/// <summary>
	/// Automatically shifts gears based on the current speed and input.
	/// </summary>
	/// <param name="useAutoShift"></param>
	private void AutoShift(bool useAutoShift = false) {
		if (!useAutoShift) return;

		//2 -> 3
		if (Speed > Gear2Speed * AutoShiftUpThreshold && CurrentGear < 3) {
			ChangeGear(3, true);
		}

		//1 -> 2
		else if (Speed > Gear1Speed * AutoShiftUpThreshold && CurrentGear < 2) {
			ChangeGear(2, true);
		}

		//0 -> 1
		else if (Mathf.Abs(Speed) < 10 && CurrentGear == 0 && Input.IsAccelerating) {
			ChangeGear(1, true);
		}

		//3 -> 2
		else if (Speed < Gear3Speed * AutoShiftDownThreshold && CurrentGear > 2) {
			ChangeGear(2, true);
		}

		//2 -> 1
		else if (Speed < Gear2Speed * AutoShiftDownThreshold && CurrentGear > 1) {
			ChangeGear(1, true);
		}

		//1 -> 0
		else if (Speed < Gear1Speed * AutoShiftDownThreshold && CurrentGear > 0) {
			ChangeGear(0, true);
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
	/// How many seconds the vehicle has been braking.
	/// </summary>
	private float BrakeTime = 0f;

	/// <summary>
	/// How many seconds it takes for <see cref="BrakeDecel"/> to reach <see cref="BrakeMaxDecel"/>.
	/// </summary>
	[Header("Braking & Deceleration")]
	public float BrakeMaxTime = 2f;

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

	private float lastSpeed;
	private float brakeCooldown = 0f;


	private void UpdateBrakingSound()
	{
		float speedDelta = lastSpeed - Speed;

		bool isBraking = speedDelta > 2f; // You can tune this threshold

		if (isBraking && brakeCooldown <= 0f)
		{
			PlayOneShotSound(brakeSound);
			brakeCooldown = 0.5f; // Prevent spam
		}

		brakeCooldown -= Time.deltaTime;
		lastSpeed = Speed;
	}

	private void PlayOneShotSound(SoundData soundData, float pitch = 1f)
{
    if (soundData != null && sfxSource != null)
    {
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(soundData.clip);
        sfxSource.pitch = 1f; // Reset to default after
    }
}


	private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.relativeVelocity.magnitude > 3f)
    {
        if (camHandler != null)
        {
            camHandler.TriggerShake();
        }
        else
        {
            Debug.LogWarning("CameraHandler is missing.");
        }

        PlayOneShotSound(crashSound); // <- Use your SoundData method
    }
}

	#endregion

	#region Game Objects & Components

	[Header("Game Objects & Components")]
	public InputManager Input;

	public Rigidbody2D Body;

	#endregion

	#region Unity Callbacks

	private void PlaySoundData(SoundData data)
{
    engineSource.clip = data.clip;
    engineSource.volume = data.volume;
    engineSource.pitch = data.pitch;
    engineSource.loop = true;
    engineSource.Play();
}


private float pitchOverrideTimer = 0f;
private float pitchOverrideValue = 1f;

	private void UpdateEngineSound()
{
	if (Speed < 0.5f) // Idle state
	{
		if (engineSource.clip != idleSound.clip)
		{
			PlaySoundData(idleSound);
		}
		engineSource.pitch = idleSound.pitch;
	}
	else
	{
		if (CurrentGear >= 1 && CurrentGear <= gearSounds.Length)
		{
			SoundData gearSound = gearSounds[CurrentGear - 1];

			if (engineSource.clip != gearSound.clip)
			{
				PlaySoundData(gearSound);
			}
		}

		// Apply pitch override if active
		if (pitchOverrideTimer > 0f)
		{
			engineSource.pitch = pitchOverrideValue;
			pitchOverrideTimer -= Time.deltaTime;
		}
		else
		{
			engineSource.pitch = Mathf.Lerp(1f, 2f, Mathf.Clamp01(Tachometer));
		}
	}
}



	private IEnumerator TemporarilyLowerEnginePitch()
	{
		engineSource.pitch = 0.8f;
		yield return new WaitForSeconds(0.3f);
		engineSource.pitch = Mathf.Lerp(1f, 2f, Tachometer); // Restore normal pitch
	}


	private void Start()
	{
		previousGear = CurrentGear;
		lastSpeed = Speed;

	}
	
		private void Update()
	{

		#region Inputs

		if (Input.IsDecelerating) FootBrake();
		else if (Input.IsHandBraking) HandBrake();
		else if (Input.IsAccelerating) Accelerate();
		else
		{
			NaturallyDecelerate();
			BrakeTime = 0;

		}

		if (!Input.IsDecelerating && Mathf.Abs(Speed) < 0.1f)
		{
			BrakeTime = 0;
			BrakeDecel = 0;
		}

		//Inputs are handled in the method itself.
		Steer();

		if (Input.IsGearingUp) ShiftGear(1, true);
		if (Input.IsGearingDown) ShiftGear(-1, true);

		if (Input.SelectGear1) ChangeGear(1, true);
		if (Input.SelectGear2) ChangeGear(2, true);
		if (Input.SelectGear3) ChangeGear(3, true);
		if (Input.SelectGearR) ChangeGear(-1, true);



		if (CurrentGear != previousGear)
		{
			bool isDownshift = CurrentGear < previousGear;

			PlayOneShotSound(gearShiftSound, isDownshift ? 0.9f : 1.1f);
			previousGear = CurrentGear;

			if (isDownshift)
			{
				StartCoroutine(TemporarilyLowerEnginePitch());
			}
		}
		#endregion


		AutoShift(UseAutoShift);

		UpdateTachometer();
		UpdateBrakingSound();
		UpdateEngineSound();
	}

	private void FixedUpdate() {
		if (Speed != 0) Body.linearVelocity = transform.up * Speed * Time.fixedDeltaTime;
		else Body.linearVelocity = Vector2.zero;

		// Project current velocity onto the forward vector only
		float forwardSpeed = Vector2.Dot(Body.linearVelocity, transform.up);
		Body.linearVelocity = transform.up * forwardSpeed;

		SteerPhysics();
	}

	#endregion
}
