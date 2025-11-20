using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("Slime Prefabs")]
    public GameObject greenSlimePrefab;
    public GameObject redSlimePrefab;
    public GameObject blueSlimePrefab;

    [Header("Spawn Timing")]
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

    [Header("Spawn Positioning")]
    public float spawnX = 12f;

    // Grounded slimes
    public float groundY = -3f;

    // Blue Slime sky range
    public float blueMinY = 3f;
    public float blueMaxY = 7f;

    private float nextSpawnTime;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomSlime();
            ScheduleNextSpawn();
        }
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
    }

    void SpawnRandomSlime()
    {
        // 0 = Green, 1 = Red, 2 = Blue
        int choice = Random.Range(0, 3);

        switch (choice)
        {
            case 0:
                SpawnGreenSlime();
                break;
            case 1:
                SpawnRedSlime();
                break;
            case 2:
                SpawnBlueSlime();
                break;
        }
    }

    void SpawnGreenSlime()
    {
        Vector3 pos = new Vector3(spawnX, groundY, 0);
        Instantiate(greenSlimePrefab, pos, Quaternion.identity);
    }

    void SpawnRedSlime()
    {
        Vector3 pos = new Vector3(spawnX, groundY, 0);
        Instantiate(redSlimePrefab, pos, Quaternion.identity);
    }

    void SpawnBlueSlime()
    {
        float spawnY = Random.Range(blueMinY, blueMaxY);
        Vector3 pos = new Vector3(spawnX, spawnY, 0);
        Instantiate(blueSlimePrefab, pos, Quaternion.identity);
    }
}
