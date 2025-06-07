using UnityEngine;

public class PlayerAutoMove : MonoBehaviour
{
    public float moveSpeed = 5f; // Units per second

    void Update()
    {
        // Move the player up every frame based on moveSpeed
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}
