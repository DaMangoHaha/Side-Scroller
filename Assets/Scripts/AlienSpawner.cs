using UnityEngine;

public class AlienSpawner : MonoBehaviour
{
    public GameObject alienPrefab;
    public float spawnX = 12f;       // X position just off-screen right
    public float groundY = -3f;      // align above the ground

    [Header("Spawn Timing")]
    public float minSpawnInterval = 1.5f; // minimum seconds between spawns
    public float maxSpawnInterval = 4f;   // maximum seconds between spawns

    [Header("Cluster Settings")]
    public float clusterSpacing = 1.5f;   // spacing between spikes in a cluster
    public int minClusterSize = 1;        // minimum spikes per cluster
    public int maxClusterSize = 3;        // maximum spikes per cluster

    private float timer;
    private float nextSpawnTime;

    void Start()
    {
        // pick a random time for the first spawn
        ScheduleNextSpawn();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnAlienCluster();
            timer = 0f;
            ScheduleNextSpawn();
        }
    }

    /// <summary>
    /// Schedules the next spawn time, applying difficulty modifier.
    /// </summary>
    void ScheduleNextSpawn()
    {
        float baseInterval = Random.Range(minSpawnInterval, maxSpawnInterval);

        // Apply difficulty modifier (lower = faster spawns)
        if (DifficultyManager.Instance != null)
        {
            baseInterval *= DifficultyManager.Instance.spawnRateModifier;
        }

        nextSpawnTime = baseInterval;
    }

    void SpawnAlienCluster()
    {
        // choose a random cluster size
        int count = Random.Range(minClusterSize, maxClusterSize + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(spawnX + (i * clusterSpacing), groundY, 0);
            Instantiate(alienPrefab, spawnPos, Quaternion.identity);
        }
    }
}

