using UnityEngine;
using System.Collections.Generic;

public class VerticalWorldSpawner : MonoBehaviour
{
    public GameObject[] worldPrefabs;         // 0-2 main, 3-5 transition
    public Transform player;
    public float spawnDistanceAhead = 100f;
    public float despawnDistance = 60f;
    public float worldHeight = 50f;

    private float nextSpawnY = 0f;
    private List<GameObject> spawnedWorlds = new List<GameObject>();
    private int lastPrefabIndex = -1;
    private int forcedPrefab2Count = 0;  // NEW: Counter for forced spawns of element 2

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnWorld();
        }
    }

    void Update()
    {
        while (player.position.y + spawnDistanceAhead > nextSpawnY)
        {
            SpawnWorld();
        }

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
        int prefabIndex;

        // Force element 2 for first two spawns
        if (forcedPrefab2Count < 2)
        {
            prefabIndex = 2;
            forcedPrefab2Count++;
        }
        else if (lastPrefabIndex == -1)
        {
            // After forced spawns, start with standard logic
            prefabIndex = Random.Range(0, 3);
        }
        else if (lastPrefabIndex == 0)
        {
            prefabIndex = Random.Range(3, 6);
        }
        else if (lastPrefabIndex == 1 || lastPrefabIndex == 2)
        {
            prefabIndex = Random.Range(0, 3);
        }
        else if (lastPrefabIndex == 3)
        {
            prefabIndex = Random.Range(0, 3);
        }
        else if (lastPrefabIndex == 4 || lastPrefabIndex == 5)
        {
            prefabIndex = Random.Range(3, 6);
        }
        else
        {
            prefabIndex = Random.Range(0, worldPrefabs.Length);
        }

        Vector3 spawnPos = new Vector3(0f, nextSpawnY, 0f);
        GameObject newWorld = Instantiate(worldPrefabs[prefabIndex], spawnPos, Quaternion.identity);
        spawnedWorlds.Add(newWorld);

        lastPrefabIndex = prefabIndex;
        nextSpawnY += worldHeight;
    }
}
