using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public Transform target;
    public Vector2 Offset = new(0, 20);
    public float FollowSpeed = 5f;
    public bool RotateWithTarget = false;
    public float RotationDamping = 5f;

    [Header("Clamp Settings")]
    public bool DoClamping = false;
    public Vector2 MinClamp;
    public Vector2 MaxClamp;

    [Header("Camera Shake")]
    public float ShakeDuration = 0.3f;
    public float ShakeMagnitude = 0.3f;

    private Vector2 CurrentVelocity = Vector3.zero;
    private Vector2 ShakeOffset = Vector3.zero;
    private float ShakeTimeRemaining = 0f;

    void LateUpdate() {
         if (target == null) return;

		// Use world-space offset for top-down view
		Vector2 desiredPosition = (Vector2) target.position + Offset;

		// Clamp position
		if (DoClamping) {
			desiredPosition.x = Mathf.Clamp(desiredPosition.x, MinClamp.x, MaxClamp.x);
			desiredPosition.y = Mathf.Clamp(desiredPosition.y, MinClamp.y, MaxClamp.y);
		}

		// Smooth follow
		transform.position = Vector2.SmoothDamp(transform.position, desiredPosition, ref CurrentVelocity, 1f / FollowSpeed);
		transform.position = new(transform.position.x, transform.position.y, -10f); // Ensure camera is always in front of the target in 2D space

		// Camera shake
		if (ShakeTimeRemaining > 0) {
			ShakeOffset = Random.insideUnitSphere * ShakeMagnitude;
			ShakeOffset.y = 0f;
			transform.position += (Vector3) ShakeOffset;
			ShakeTimeRemaining -= Time.deltaTime;
		}

		// Rotate with vehicle (optional for top-down)
		if (RotateWithTarget) {
			Quaternion desiredRotation = Quaternion.Euler(0f, target.eulerAngles.y, 90f);
			transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, RotationDamping * Time.deltaTime);
		}

		else {
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
    }

    // Call this to trigger shake externally
    public void TriggerShake(float duration = -1f, float magnitude = -1f) {
        ShakeTimeRemaining = (duration > 0f) ? duration : ShakeDuration;
        ShakeMagnitude = (magnitude > 0f) ? magnitude : ShakeMagnitude;
        Debug.Log("Collision detected, camera shake triggered.");
    }




    /*
          to be used on the vehicle script
          
        Scaling for shake intensity based on collision speed
            cam.TriggerShake(0.2f, collision.relativeVelocity.magnitude * 0.05f);

    void OnCollisionEnter(Collision collision)
    {
    // Only shake on significant collisions (like terrain, walls, etc.)
    if (collision.relativeVelocity.magnitude > 3f)
    {
        CameraHandler cam = Camera.main.GetComponent<CameraHandler>();
        if (cam != null)
        {
            cam.TriggerShake();
        }
     }
    }


    */
}
