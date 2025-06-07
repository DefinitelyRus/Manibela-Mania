using UnityEngine;
using System.Collections.Generic;

public class VerticalWorldSpawner : MonoBehaviour
{
    public GameObject[] worldPrefabs;         // World1 to World6
    public Transform player;                  // Reference to the player
    public float spawnDistanceAhead = 100f;   // Distance ahead of the player to spawn new worlds
    public float despawnDistance = 60f;       // Distance below player to despawn old worlds
    public float worldHeight = 50f;           // Vertical size of each world prefab

    private float nextSpawnY = 0f;            // Next Y coordinate to spawn at
    private List<GameObject> spawnedWorlds = new List<GameObject>();

    void Start()
    {
        // Initial spawn
        for (int i = 0; i < 3; i++)
        {
            SpawnWorld();
        }
    }

    void Update()
    {
        // Spawn new worlds ahead of player
        while (player.position.y + spawnDistanceAhead > nextSpawnY)
        {
            SpawnWorld();
        }

        // Despawn worlds behind the player
        for (int i = spawnedWorlds.Count - 1; i >= 0; i--)
        {
            if (player.position.y - spawnedWorlds[i].transform.position.y > despawnDistance)
            {
                Destroy(spawnedWorlds[i]);
                spawnedWorlds.RemoveAt(i);
            }
        }
    }

    void SpawnWorld()
    {
        GameObject prefab = worldPrefabs[Random.Range(0, worldPrefabs.Length)];
        Vector3 spawnPos = new Vector3(0f, nextSpawnY, 0f);
        GameObject newWorld = Instantiate(prefab, spawnPos, Quaternion.identity);
        spawnedWorlds.Add(newWorld);

        nextSpawnY += worldHeight;
    }
}
