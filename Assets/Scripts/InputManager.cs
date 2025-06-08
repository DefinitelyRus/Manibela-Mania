using UnityEngine;

public class InputManager : MonoBehaviour
{
	[Header("Movement Controls")]
	public KeyCode MoveForward = KeyCode.W;
	public KeyCode MoveBackward = KeyCode.S;
	public KeyCode MoveLeft = KeyCode.A;
	public KeyCode MoveRight = KeyCode.D;
	public KeyCode Handbrake = KeyCode.Space;
	public KeyCode GearUp = KeyCode.Q;
	public KeyCode GearDown = KeyCode.E;
	public KeyCode Gear1 = KeyCode.Alpha1;
	public KeyCode Gear2 = KeyCode.Alpha2;
	public KeyCode Gear3 = KeyCode.Alpha3;
	public KeyCode GearR = KeyCode.R;
	public KeyCode Honk = KeyCode.H;

	public bool IsAccelerating { get; protected set; } = false;
	public bool IsDecelerating { get; protected set; } = false;
	public bool IsHandBraking { get; protected set; } = false;
	public bool OnHandBrake { get; protected set; } = false;
	public bool OnGearUp { get; protected set; } = false;
	public bool OnGearDown { get; protected set; } = false;
	public bool OnGear1 { get; protected set; } = false;
	public bool OnGear2 { get; protected set; } = false;
	public bool OnGear3 { get; protected set; } = false;
	public bool OnGearR { get; protected set; } = false;
	public bool OnHonk { get; protected set; } = false;
	public float Steering { get; protected set; } = 0;

	[Header("Menu Controls")]
	public KeyCode MenuAccept = KeyCode.Return;
	public KeyCode MenuCancel = KeyCode.Escape;

	public bool OnAccept { get; protected set; } = false;
	public bool OnCancel { get; protected set; } = false;

	[Header("Mouse Controls")]
	public KeyCode MouseLeft = KeyCode.Mouse0;
	public KeyCode MouseRight = KeyCode.Mouse1;

	public bool OnMouse1 { get; protected set; } = false;
	public bool OnMouse2 { get; protected set; } = false;

	public virtual void InputListener() {

		#region Movement

		IsAccelerating = Input.GetKey(MoveForward);
		IsDecelerating = Input.GetKey(MoveBackward);
		IsHandBraking = Input.GetKey(Handbrake);
		OnHandBrake = Input.GetKeyDown(Handbrake);

		if (Input.GetKey(MoveLeft)) Steering = 1;
		else if (Input.GetKey(MoveRight)) Steering = -1;
		else if (Input.GetKey(MoveLeft) && Input.GetKey(MoveRight)) Steering = 0;
		else Steering = 0;

		#endregion

		#region Gears

		OnGearUp = Input.GetKeyDown(GearUp);
		OnGearDown = Input.GetKeyDown(GearDown);

		OnGear1 = Input.GetKeyDown(Gear1);
		OnGear2 = Input.GetKeyDown(Gear2);
		OnGear3 = Input.GetKeyDown(Gear3);
		OnGearR = Input.GetKeyDown(GearR);

		#endregion

		#region Menu Controls

		OnAccept = Input.GetKeyDown(MenuAccept);
		OnCancel = Input.GetKeyDown(MenuCancel);

		#endregion

		#region Mouse Controls

		OnMouse1 = Input.GetKeyDown(MouseLeft);
		OnMouse2 = Input.GetKeyDown(MouseRight);

		#endregion
	}

	private void Update() {
		InputListener();
	}
}
