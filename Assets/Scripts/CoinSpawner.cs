using UnityEngine;

public class CoinSpawner : MonoBehaviour
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
        int pattern = Random.Range(0, 8); // 0–7

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
            case 3:
                SpawnDiamond();
                break;
            case 4:
                SpawnZigzag();
                break;
            case 5:
                SpawnCircle();
                break;
            case 6:
                SpawnCross();
                break;
            case 7:
                SpawnWave();
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

    void SpawnStair()
    {
        int steps = Random.Range(3, 5); // 3–4 coins
        float stepHeight = 0.5f;
        float stepSpacing = .9f;

        int index = Random.Range(0, coinPrefabs.Length);
        float startY = Random.Range(spawnYMin, spawnYMax - steps * stepHeight);

        // Randomly decide ascending or descending
        int direction = (Random.value > 0.5f) ? 1 : -1;

        for (int i = 0; i < steps; i++)
        {
            Vector3 pos = new Vector3(spawnX + i * stepSpacing, startY + (i * stepHeight * direction), 0);
            GameObject coin = Instantiate(coinPrefabs[index], pos, Quaternion.identity);
            TryAddFloat(coin);
        }
    }

    void SpawnArc()
    {
        int count = 6; // number of coins in arc
        float radius = 2f; // curve size
        float angleStep = Mathf.PI / (count - 1); // semi-circle

        int index = Random.Range(0, coinPrefabs.Length);
        float baseY = Random.Range(spawnYMin + radius, spawnYMax - radius);

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float yOffset = Mathf.Sin(angle) * radius;
            float xOffset = i * 0.8f;

            Vector3 pos = new Vector3(spawnX + xOffset, baseY + yOffset, 0);
            GameObject coin = Instantiate(coinPrefabs[index], pos, Quaternion.identity);
            TryAddFloat(coin);
        }
    }

    /// <summary>
    /// Spawns coins in a diamond/rhombus shape.
    /// Looks like a collectible gem floating in the air.
    /// </summary>
    void SpawnDiamond()
    {
        int index = Random.Range(0, coinPrefabs.Length);
        float centerY = Random.Range(spawnYMin + 1f, spawnYMax - 1f);
        float spacing = 0.7f;

        // Diamond point offsets: top, right, bottom, left, and center
        Vector2[] offsets = new Vector2[]
        {
            new Vector2(0, 0),             // center
            new Vector2(0, spacing),       // top
            new Vector2(spacing, 0),       // right
            new Vector2(0, -spacing),      // bottom
            new Vector2(-spacing, 0),      // left
            new Vector2(spacing, spacing),  // top-right
            new Vector2(spacing, -spacing), // bottom-right
            new Vector2(-spacing, spacing), // top-left
            new Vector2(-spacing, -spacing) // bottom-left
        };

        foreach (Vector2 offset in offsets)
        {
            Vector3 pos = new Vector3(spawnX + offset.x, centerY + offset.y, 0);
            GameObject coin = Instantiate(coinPrefabs[index], pos, Quaternion.identity);
            TryAddFloat(coin);
        }
    }

    /// <summary>
    /// Spawns coins in a zigzag pattern, alternating up and down.
    /// Great for making the player bob up and down to collect them.
    /// </summary>
    void SpawnZigzag()
    {
        int count = Random.Range(4, 7); // 4–6 coins
        float xSpacing = 0.9f;
        float yAmplitude = 0.8f;

        int index = Random.Range(0, coinPrefabs.Length);
        float baseY = Random.Range(spawnYMin + yAmplitude, spawnYMax - yAmplitude);

        for (int i = 0; i < count; i++)
        {
            // Alternate between up and down each step
            float yOffset = (i % 2 == 0) ? yAmplitude : -yAmplitude;
            Vector3 pos = new Vector3(spawnX + i * xSpacing, baseY + yOffset, 0);
            GameObject coin = Instantiate(coinPrefabs[index], pos, Quaternion.identity);
            TryAddFloat(coin);
        }
    }

    /// <summary>
    /// Spawns coins in a full circle/ring shape.
    /// Eye-catching pattern that rewards flying through the center.
    /// </summary>
    void SpawnCircle()
    {
        int count = 8;
        float radius = 1.2f;

        int index = Random.Range(0, coinPrefabs.Length);
        float centerY = Random.Range(spawnYMin + radius, spawnYMax - radius);
        float angleStep = (2f * Mathf.PI) / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float xOffset = Mathf.Cos(angle) * radius;
            float yOffset = Mathf.Sin(angle) * radius;

            Vector3 pos = new Vector3(spawnX + xOffset, centerY + yOffset, 0);
            GameObject coin = Instantiate(coinPrefabs[index], pos, Quaternion.identity);
            TryAddFloat(coin);
        }
    }

    /// <summary>
    /// Spawns coins in a cross/plus shape.
    /// A compact pattern that covers vertical and horizontal space.
    /// </summary>
    void SpawnCross()
    {
        int index = Random.Range(0, coinPrefabs.Length);
        float centerY = Random.Range(spawnYMin + 1.5f, spawnYMax - 1.5f);
        float spacing = 0.7f;
        int armLength = 2; // coins extending from center in each direction

        // Center coin
        Vector3 centerPos = new Vector3(spawnX, centerY, 0);
        GameObject centerCoin = Instantiate(coinPrefabs[index], centerPos, Quaternion.identity);
        TryAddFloat(centerCoin);

        // Horizontal and vertical arms
        for (int i = 1; i <= armLength; i++)
        {
            // Right
            SpawnCoinAt(index, new Vector3(spawnX + i * spacing, centerY, 0));
            // Left
            SpawnCoinAt(index, new Vector3(spawnX - i * spacing, centerY, 0));
            // Up
            SpawnCoinAt(index, new Vector3(spawnX, centerY + i * spacing, 0));
            // Down
            SpawnCoinAt(index, new Vector3(spawnX, centerY - i * spacing, 0));
        }
    }

    /// <summary>
    /// Spawns coins along a smooth sine wave path.
    /// Creates a flowing, natural-looking trail for the player to follow.
    /// </summary>
    void SpawnWave()
    {
        int count = Random.Range(6, 9); // 6–8 coins
        float xSpacing = 0.8f;
        float waveHeight = 1.2f;
        float waveFrequency = 1.5f; // how tight the wave is

        int index = Random.Range(0, coinPrefabs.Length);
        float baseY = Random.Range(spawnYMin + waveHeight, spawnYMax - waveHeight);

        for (int i = 0; i < count; i++)
        {
            float yOffset = Mathf.Sin(i * waveFrequency) * waveHeight;
            Vector3 pos = new Vector3(spawnX + i * xSpacing, baseY + yOffset, 0);
            GameObject coin = Instantiate(coinPrefabs[index], pos, Quaternion.identity);
            TryAddFloat(coin);
        }
    }

    /// <summary>Helper to spawn a single coin at a position and apply float chance.</summary>
    void SpawnCoinAt(int prefabIndex, Vector3 position)
    {
        GameObject coin = Instantiate(coinPrefabs[prefabIndex], position, Quaternion.identity);
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
