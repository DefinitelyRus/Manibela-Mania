using UnityEngine;

public class Tracker : MonoBehaviour
{
    public Transform car; // Assign the car in the Inspector

    [Header("X Clamp Settings")]
    public bool useXClamp = false;
    public float minX = -10f;
    public float maxX = 10f;

    void Update()
    {
        Vector3 trackerPos = transform.position;
        Vector3 carPos = car.position;

        // X follows car, optionally clamped
        trackerPos.x = carPos.x;
        if (useXClamp)
        {
            trackerPos.x = Mathf.Clamp(trackerPos.x, minX, maxX);
        }

        // Y and Z follow car directly
        trackerPos.y = carPos.y;
        trackerPos.z = carPos.z;

        transform.position = trackerPos;
    }
}
