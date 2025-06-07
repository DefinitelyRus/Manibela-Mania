using UnityEngine;

public class Sakay : MonoBehaviour
{
    public string characterName = "char1"; // Set this in the Inspector or dynamically when spawning

    private float timer = 0f;
    private bool playerInside = false;

    // Change from OnTriggerEnter to OnTriggerEnter2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered 2D trigger!"); // Added for debugging
            playerInside = true;
            timer = 0f; // reset timer on entry
        }
    }

    // Change from OnTriggerExit to OnTriggerExit2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited 2D trigger!"); // Added for debugging
            playerInside = false;
            timer = 0f;
        }
    }

    private void Update()
    {
        if (playerInside)
        {
            timer += Time.deltaTime;
            Debug.Log($"Player inside: {timer:F2}s");

            if (timer >= 3f)
            {
                Debug.Log("Player stayed for 3 seconds. Destroying object.");
                // Ensure GlobalCounter is also set up for your 2D project if it's a separate script
                GlobalCounter.Increment(characterName);
                Destroy(gameObject);
            }
        }
    }
}