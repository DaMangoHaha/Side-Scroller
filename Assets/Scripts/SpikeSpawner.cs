using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    public GameObject spikePrefab;
    public float spawnX = 12f;       // X position just off-screen right
    public float groundY = -3f;      // align with ground level

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
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnSpikeCluster();
            timer = 0f;
            nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    void SpawnSpikeCluster()
    {
        // choose a random cluster size
        int count = Random.Range(minClusterSize, maxClusterSize + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(spawnX + (i * clusterSpacing), groundY, 0);
            Instantiate(spikePrefab, spawnPos, Quaternion.identity);
        }
    }
}


