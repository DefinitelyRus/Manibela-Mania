using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 20, 0);
    public float followSpeed = 5f;
    public bool rotateWithTarget = false;
    public float rotationDamping = 5f;

    [Header("Clamp Settings")]
    public bool useClamping = false;
    public Vector2 minClamp;
    public Vector2 maxClamp;

    [Header("Camera Shake")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.3f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeTimeRemaining = 0f;

    void LateUpdate()
    {
         if (target == null) return;

    // Use world-space offset for top-down view
    Vector3 desiredPosition = target.position + offset;

    // Clamp position
    if (useClamping)
    {
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minClamp.x, maxClamp.x);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, minClamp.y, maxClamp.y);
    }

    // Smooth follow
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);

    // Camera shake
    if (shakeTimeRemaining > 0)
    {
        shakeOffset = Random.insideUnitSphere * shakeMagnitude;
        shakeOffset.y = 0f;
        transform.position += shakeOffset;
        shakeTimeRemaining -= Time.deltaTime;
    }

    // Rotate with vehicle (optional for top-down)
    if (rotateWithTarget)
    {
        Quaternion desiredRotation = Quaternion.Euler(360f, target.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, rotationDamping * Time.deltaTime);
    }
    else
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
    }

    // Call this to trigger shake externally
    public void TriggerShake(float duration = -1f, float magnitude = -1f)
    {
        shakeTimeRemaining = (duration > 0f) ? duration : shakeDuration;
        shakeMagnitude = (magnitude > 0f) ? magnitude : shakeMagnitude;
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
