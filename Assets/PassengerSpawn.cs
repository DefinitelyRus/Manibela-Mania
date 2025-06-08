using UnityEngine;

public class PassengerSpawn : MonoBehaviour
{
    [Tooltip("Passenger prefabs to choose from (should be 5).")]
    public GameObject[] passengerPrefabs;

    [Range(0f, 100f)]
    [Tooltip("Chance to spawn a passenger on creation (e.g. 0.2 = 0.2%)")]
    public float spawnChancePercent = 15f;

    void Start()
    {
        if (passengerPrefabs == null || passengerPrefabs.Length == 0)
        {
            Debug.LogWarning("No passenger prefabs assigned!");
            return;
        }

        float roll = Random.Range(0f, 100f);
        Debug.Log($"Spawn roll: {roll}, spawn if < {spawnChancePercent}");

        if (roll < spawnChancePercent)
        {
            int index = Random.Range(0, passengerPrefabs.Length);
            GameObject passenger = Instantiate(passengerPrefabs[index], transform.position, Quaternion.identity);
            Debug.Log($"Spawned passenger: {passenger.name}");
        }
    }
}