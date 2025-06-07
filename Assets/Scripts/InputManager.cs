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

	public bool IsAccelerating { get; private set; } = false;
	public bool IsDecelerating { get; private set; } = false;
	public bool IsHandBraking { get; private set; } = false;
	public bool IsGearingUp { get; private set; } = false;
	public bool IsGearingDown { get; private set; } = false;
	public bool SelectGear1 { get; private set; } = false;
	public bool SelectGear2 { get; private set; } = false;
	public bool SelectGear3 { get; private set; } = false;
	public bool SelectGearR { get; private set; } = false;
	public float Steering { get; private set; } = 0;

	[Header("Menu Controls")]
	public KeyCode MenuAccept = KeyCode.Return;
	public KeyCode MenuCancel = KeyCode.Escape;

	public bool PressedAccept { get; private set; } = false;
	public bool PressedCancel { get; private set; } = false;

	[Header("Mouse Controls")]
	public KeyCode MouseLeft = KeyCode.Mouse0;
	public KeyCode MouseRight = KeyCode.Mouse1;

	public bool ClickedLeft { get; private set; } = false;
	public bool ClickedRight { get; private set; } = false;

	public void InputListener() {

		#region Movement

		IsAccelerating = Input.GetKey(MoveForward);
		IsDecelerating = Input.GetKey(MoveBackward);
		IsHandBraking = Input.GetKey(Handbrake);

		if (Input.GetKey(MoveLeft)) Steering = 1;
		else if (Input.GetKey(MoveRight)) Steering = -1;
		else if (Input.GetKeyUp(MoveLeft) || Input.GetKeyUp(MoveRight)) Steering = 0;
		else Steering = 0;

		#endregion

		#region Gears

		IsGearingUp = Input.GetKey(GearUp);
		IsGearingDown = Input.GetKeyDown(GearDown);

		SelectGear1 = Input.GetKeyDown(Gear1);
		SelectGear2 = Input.GetKeyDown(Gear2);
		SelectGear3 = Input.GetKeyDown(Gear3);
		SelectGearR = Input.GetKeyDown(GearR);

		#endregion

		#region Menu Controls

		PressedAccept = Input.GetKeyDown(MenuAccept);
		PressedCancel = Input.GetKeyDown(MenuCancel);

		#endregion

		#region Mouse Controls

		ClickedLeft = Input.GetKeyDown(MouseLeft);
		ClickedRight = Input.GetKeyDown(MouseRight);

		#endregion
	}

	private void Update() {
		InputListener();
	}
}
