using UnityEngine;

public class CupidCoinSpawner : MonoBehaviour
{
    public GameObject cupidCoinPrefab; // Single CupidCoin prefab (Red variant)
    public float spawnInterval = 2f;
    public float spawnX = 12f;
    public float spawnYMin = -2f;
    public float spawnYMax = 2f;

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
            case 1:
                SpawnStair();
                break;
            case 2:
                SpawnArc();
                break;
        }
    }

    void SpawnLine()
    {
        Vector3 spawnPos = new Vector3(spawnX, Random.Range(spawnYMin, spawnYMax), 0);
        Instantiate(cupidCoinPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnStair()
    {
        int steps = Random.Range(4, 7); // 4–6 coins
        float stepHeight = 0.5f;
        float stepSpacing = .9f;

        float startY = Random.Range(spawnYMin, spawnYMax - steps * stepHeight);

        // Randomly decide ascending or descending
        int direction = (Random.value > 0.5f) ? 1 : -1;

        for (int i = 0; i < steps; i++)
        {
            Vector3 pos = new Vector3(spawnX + i * stepSpacing, startY + (i * stepHeight * direction), 0);
            Instantiate(cupidCoinPrefab, pos, Quaternion.identity);
        }
    }

    void SpawnArc()
    {
        int count = 6; // number of coins in arc
        float radius = 2f; // curve size
        float angleStep = Mathf.PI / (count - 1); // semi-circle

        float baseY = Random.Range(spawnYMin + radius, spawnYMax - radius);

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float yOffset = Mathf.Sin(angle) * radius;
            float xOffset = i * 0.8f;

            Vector3 pos = new Vector3(spawnX + xOffset, baseY + yOffset, 0);
            Instantiate(cupidCoinPrefab, pos, Quaternion.identity);
        }
    }
}