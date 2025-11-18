using UnityEngine;

public class CoinLine : MonoBehaviour
{
    public GameObject[] coinPrefabs; // Bronze, Silver, Gold
    public float spawnInterval = 2f;
    public float spawnX = 12f;
    public float spawnYMin = -2f;
    public float spawnYMax = 2f;

    [Header("Special Movement")]
    [Range(0f, 1f)] public float floatChance = 0.3f; // chance a coin floats

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnPattern();
            timer = 0f;
        }
    }

    void SpawnPattern()
    {
        int pattern = Random.Range(0, 3); // 0 = line, 1 = stairs, 2 = arc

        switch (pattern)
        {
            case 0:
                SpawnLine();
                break;
        }
    }

    void SpawnLine()
    {
        int index = Random.Range(0, coinPrefabs.Length);
        Vector3 spawnPos = new Vector3(spawnX, Random.Range(spawnYMin, spawnYMax), 0);
        GameObject coin = Instantiate(coinPrefabs[index], spawnPos, Quaternion.identity);
        TryAddFloat(coin);
    }

    void TryAddFloat(GameObject coin)
    {
        if (Random.value < floatChance)
        {
            if (coin.GetComponent<CoinFloat>() == null)
                coin.AddComponent<CoinFloat>();
        }
    }
}

