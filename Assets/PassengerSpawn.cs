using UnityEngine;
using System.Collections.Generic;

public class PassengerSpawn : MonoBehaviour
{
    [Tooltip("Passenger prefabs to choose from (should be 5).")]
    public GameObject[] passengerPrefabs;

    [Range(0f, 100f)]
    [Tooltip("Chance to spawn a passenger on creation (e.g. 0.2 = 2%)")]
    public float spawnChancePercent = 3f;

    private PassengerCarrier passengerCarrier;

    void Start()
    {
        if (passengerPrefabs == null || passengerPrefabs.Length == 0)
        {
            Debug.LogWarning("No passenger prefabs assigned!");
            return;
        }

        // Get reference to PassengerCarrier
        passengerCarrier = GameObject.FindObjectOfType<PassengerCarrier>();
        if (passengerCarrier == null)
        {
            Debug.LogError("[PassengerSpawn] Could not find PassengerCarrier in scene!");
            return;
        }

        float roll = Random.Range(0f, 100f);

        if (roll < spawnChancePercent)
        {
            // Get list of available character indices (1-6)
            List<int> availableIndices = new List<int>();
            for (int i = 1; i <= 6; i++)
            {
                availableIndices.Add(i);
            }

            // Remove indices of characters that are already passengers
            foreach (BoardedPassenger boardedPassenger in passengerCarrier.Passengers)
            {
                // Get the character number from the passenger's seat
                if (PassengerSeating.Instance != null)
                {
                    foreach (PassengerSeating.Seat seat in PassengerSeating.Instance.seats)
                    {
                        if (seat.occupied && seat.currentPassenger != null)
                        {
                            // Extract the number from "CharX" format
                            if (int.TryParse(seat.currentPassenger.Substring(4), out int charNumber))
                            {
                                availableIndices.Remove(charNumber);
                            }
                        }
                    }
                }
            }

            // If no available characters, don't spawn
            if (availableIndices.Count == 0)
            {
                Debug.Log("[PassengerSpawn] No available characters to spawn!");
                return;
            }

            // Pick a random available character
            int randomIndex = Random.Range(0, availableIndices.Count);
            int selectedCharIndex = availableIndices[randomIndex] - 1; // Convert to 0-based index

            if (passengerPrefabs[selectedCharIndex] == null)
            {
                Debug.LogWarning($"[PassengerSpawn] Passenger prefab at index {selectedCharIndex} is null!");
                return;
            }

            // Instantiate the passenger at the carrier's position
            GameObject passenger = Instantiate(passengerPrefabs[selectedCharIndex], transform.position, Quaternion.identity);
            Debug.Log($"[PassengerSpawn] Spawned passenger: Char{selectedCharIndex + 1}");
        }
    }
}