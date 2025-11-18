using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("Slime Prefabs")]
    public GameObject greenSlimePrefab;   // Assign your GreenSlime prefab here  
    // Later you can add: public GameObject redSlimePrefab, blueSlimePrefab

    [Header("Spawn Settings")]
    public float spawnX = 12f;        // just off the right side of screen
    public float groundY = -3f;       // adjust to match level floor
    public float minSpawnTime = 2f;   // minimum delay
    public float maxSpawnTime = 5f;   // maximum delay

    private float nextSpawnTime;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnGreenSlime();
            ScheduleNextSpawn();
        }
    }

    void SpawnGreenSlime()
    {
        Vector3 spawnPos = new Vector3(spawnX, groundY, 0);
        Instantiate(greenSlimePrefab, spawnPos, Quaternion.identity);
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
    }
}

