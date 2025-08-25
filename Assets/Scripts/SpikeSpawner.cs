using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    public GameObject spikePrefab;
    public float spawnInterval = 3f;
    public float spawnX = 12f;   // off-screen right
    public float groundY = -3f;  // line up with your ground

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnSpike();
            timer = 0f;
        }
    }

    void SpawnSpike()
    {
        Vector3 spawnPos = new Vector3(spawnX, groundY, 0);
        Instantiate(spikePrefab, spawnPos, Quaternion.identity);
    }
}
