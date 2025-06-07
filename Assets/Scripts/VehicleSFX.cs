using UnityEngine;

public class VehicleSFX : MonoBehaviour
{
	[Header("Audio Sources")]
	public AudioSource Idle;

	public AudioSource GearShift;

	public AudioSource[] GearAcceleration;

	public AudioSource Handbrake;

	public AudioSource Crash;

	public AudioSource Horn;

	public enum OneShotSFX {
		GearShift,
		Handbrake,
		Crash,
		Horn
	}

	public enum LoopingSFX {
		None,
		Idle,
		Gear1,
		Gear2,
		Gear3
	}

	[Header("Volume Settings")]
	[Range(0.0f, 1.0f)]
	public float MasterVolume = 1.0f;

	[Range(0.0f, 1.0f)]
	public float IdleVolume = 1.0f;

	[Range(0.0f, 1.0f)]
	public float ShifterVolume = 1.0f;

	[Range(0.0f, 1.0f)]
	public float AccelerationVolume = 1.0f;

	[Range(0.0f, 1.0f)]
	public float HandbrakeVolume = 1.0f;

	[Range(0.0f, 1.0f)]
	public float CrashVolume = 1.0f;

	[Range(0.0f, 1.0f)]
	public float HornVolume = 1.0f;

	[Header("Playing Audio")]
	public VehicleMovement Movement;

	public float TransitionTime = 0.5f;

	private float Timer = 0;

	private bool DoneTransitioning = false;

	public LoopingSFX PreviousLoopingSFX = LoopingSFX.None;

	public LoopingSFX CurrentLoopingSFX = LoopingSFX.Idle;

	public void SetLooping(LoopingSFX sfx) {
		if (CurrentLoopingSFX == sfx) return;

		PreviousLoopingSFX = CurrentLoopingSFX;
		CurrentLoopingSFX = sfx;

		Timer = 0;
		DoneTransitioning = false;
	}

	public void Transition() {
		if (DoneTransitioning) return;

		Timer += Time.deltaTime;
		Timer = Mathf.Clamp(Timer, 0, TransitionTime);

		float fromSfxVolume = TransitionTime - Timer / TransitionTime;
		float toSfxVolume = Timer / TransitionTime;
		fromSfxVolume = Mathf.Clamp(fromSfxVolume, 0, 1);
		toSfxVolume = Mathf.Clamp(toSfxVolume, 0, 1);

		switch (PreviousLoopingSFX) {
			case LoopingSFX.None: break;
			case LoopingSFX.Idle:
				Idle.volume = IdleVolume * fromSfxVolume * MasterVolume;
				break;
			case LoopingSFX.Gear1:
				GearAcceleration[0].volume = AccelerationVolume * fromSfxVolume * MasterVolume;
				break;
			case LoopingSFX.Gear2:
				GearAcceleration[1].volume = AccelerationVolume * fromSfxVolume * MasterVolume;
				break;
			case LoopingSFX.Gear3:
				GearAcceleration[2].volume = AccelerationVolume * fromSfxVolume * MasterVolume;
				break;
		}

		switch (CurrentLoopingSFX) {
			case LoopingSFX.None: break;
			case LoopingSFX.Idle:
				Idle.volume = IdleVolume * toSfxVolume * MasterVolume;
				break;
			case LoopingSFX.Gear1:
				GearAcceleration[0].volume = AccelerationVolume * toSfxVolume * MasterVolume;
				break;
			case LoopingSFX.Gear2:
				GearAcceleration[1].volume = AccelerationVolume * toSfxVolume * MasterVolume;
				break;
			case LoopingSFX.Gear3:
				GearAcceleration[2].volume = AccelerationVolume * toSfxVolume * MasterVolume;
				break;
		}

		//Prevent further transitions
		if (Timer >= TransitionTime) {
			DoneTransitioning = true;
			return;
		}
	}

	public void Play(OneShotSFX sfx) {
		switch (sfx) {
			case OneShotSFX.GearShift:
				GearShift.volume = ShifterVolume * MasterVolume;
				GearShift.Play();
				break;
			case OneShotSFX.Handbrake:
				Handbrake.volume = HandbrakeVolume * MasterVolume;
				Handbrake.Play();
				break;
			case OneShotSFX.Crash:
				Crash.volume = CrashVolume * MasterVolume;
				Crash.Play();
				break;
			case OneShotSFX.Horn:
				Horn.volume = HornVolume * MasterVolume;
				Horn.Play();
				break;
		}
	}

	private void UpdateEnginePitch() {
		switch(Movement.CurrentGear) {
			case 0:
				Idle.pitch = 1.0f;
				break;
			case 1:
				GearAcceleration[0].pitch = 1.0f + Movement.Tachometer * 0.5f;
				break;
			case 2:
				GearAcceleration[1].pitch = 1.0f + Movement.Tachometer * 0.5f;
				break;
			case 3:
				GearAcceleration[2].pitch = 1.0f + Movement.Tachometer * 0.5f;
				break;
			case -1:
				Idle.pitch = 1.0f;
				break;
		}
	}

	private void Start() {
		Idle.Play();
		GearAcceleration[0].Play();
		GearAcceleration[1].Play();
		GearAcceleration[2].Play();
	}

	private void Update() {
		Transition();
		UpdateEnginePitch();
	}
}
