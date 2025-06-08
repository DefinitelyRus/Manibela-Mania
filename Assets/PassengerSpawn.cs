using UnityEngine;

public class PassengerSpawn : MonoBehaviour
{
    [Tooltip("Passenger prefabs to choose from (should be 5).")]
    public GameObject[] passengerPrefabs;

    [Range(0f, 100f)]
    [Tooltip("Chance to spawn a passenger on creation (e.g. 0.2 = 2%)")]
    public float spawnChancePercent = 3f;

    void Start()
    {
        if (passengerPrefabs == null || passengerPrefabs.Length == 0)
        {
            Debug.LogWarning("No passenger prefabs assigned!");
            return;
        }

        float roll = Random.Range(0f, 100f);

        if (roll < spawnChancePercent)
        {
            int index = Random.Range(0, passengerPrefabs.Length);

			if (passengerPrefabs[index] == null) {
				Debug.LogWarning($"[PassengerSpawn] Passenger prefab at index {index} is null!");
				return;
			}

			// Instantiate the passenger at the carrier's position
			GameObject passenger = Instantiate(passengerPrefabs[index], transform.position, Quaternion.identity);

            //Debug.Log($"[PassengerSpawn] Spawned passenger: {passenger.name}");
        }
    }
}