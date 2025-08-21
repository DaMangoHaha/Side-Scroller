using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject[] coinPrefabs; // assign Bronze, Silver, Gold
    public float spawnInterval = 2f;
    public float spawnYMin = -2f;
    public float spawnYMax = 2f;
    public float spawnX = 12f; // just off the right side of the screen

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCoin();
            timer = 0f;
        }
    }

    void SpawnCoin()
    {
        int index = Random.Range(0, coinPrefabs.Length);
        Vector3 spawnPos = new Vector3(spawnX, Random.Range(spawnYMin, spawnYMax), 0);
        Instantiate(coinPrefabs[index], spawnPos, Quaternion.identity);
    }
}

