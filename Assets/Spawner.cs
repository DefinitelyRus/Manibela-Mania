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
    private int forcedPrefab2Count = 0;  // Counter for forced spawns of element 2
    private bool isSpawning = false;

    void Start()
    {
        if (worldPrefabs == null || worldPrefabs.Length == 0)
        {
            Debug.LogError("No world prefabs assigned! Please assign world prefabs in the Inspector.");
            return;
        }

        // Initial spawn of first three areas
        for (int i = 0; i < 3; i++)
        {
            SpawnWorld();
        }
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("Player transform not assigned! Please assign the player in the Inspector.");
            return;
        }

        // Spawn new areas as player moves up
        if (!isSpawning && player.position.y + spawnDistanceAhead > nextSpawnY)
        {
            isSpawning = true;
            SpawnWorld();
            isSpawning = false;
        }

        // Despawn old areas
        for (int i = spawnedWorlds.Count - 1; i >= 0; i--)
        {
            if (spawnedWorlds[i] == null)
            {
                spawnedWorlds.RemoveAt(i);
                continue;
            }

            if (player.position.y - spawnedWorlds[i].transform.position.y > despawnDistance)
            {
                Destroy(spawnedWorlds[i]);
                spawnedWorlds.RemoveAt(i);
            }
        }
    }

    void SpawnWorld()
    {
        if (worldPrefabs == null || worldPrefabs.Length == 0)
        {
            Debug.LogError("No world prefabs assigned! Please assign world prefabs in the Inspector.");
            return;
        }

        int prefabIndex;

        // Force element 2 for first two spawns
        if (forcedPrefab2Count < 2)
        {
            prefabIndex = 2;
            forcedPrefab2Count++;
            Debug.Log($"Forced spawn of prefab 2 (count: {forcedPrefab2Count})");
        }
        else if (lastPrefabIndex == -1)
        {
            // After forced spawns, start with standard logic
            prefabIndex = Random.Range(0, 3);
            Debug.Log("First random spawn after forced spawns");
        }
        else if (lastPrefabIndex == 0)
        {
            prefabIndex = Random.Range(3, 6);
            Debug.Log("Transition spawn after main area");
        }
        else if (lastPrefabIndex == 1 || lastPrefabIndex == 2)
        {
            prefabIndex = Random.Range(0, 3);
            Debug.Log("Main area spawn after main/transition");
        }
        else if (lastPrefabIndex == 3)
        {
            prefabIndex = Random.Range(0, 3);
            Debug.Log("Main area spawn after transition");
        }
        else if (lastPrefabIndex == 4 || lastPrefabIndex == 5)
        {
            prefabIndex = Random.Range(3, 6);
            Debug.Log("Transition spawn after transition");
        }
        else
        {
            prefabIndex = Random.Range(0, worldPrefabs.Length);
            Debug.Log("Fallback random spawn");
        }

        Vector3 spawnPos = new Vector3(0f, nextSpawnY, 0f);
        GameObject newWorld = Instantiate(worldPrefabs[prefabIndex], spawnPos, Quaternion.identity);
        
        if (newWorld != null)
        {
            spawnedWorlds.Add(newWorld);
            lastPrefabIndex = prefabIndex;
            nextSpawnY += worldHeight;

            // Increment the area counter when a new area is spawned
            Sakay.IncrementArea();
            Debug.Log($"Spawned new area at Y: {spawnPos.y}, Area: {Sakay.currentArea}");
        }
        else
        {
            Debug.LogError($"Failed to instantiate world prefab at index {prefabIndex}");
        }
    }
}
