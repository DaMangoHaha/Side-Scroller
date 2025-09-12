using UnityEngine;

public class EnergyPotionSpawner : MonoBehaviour
{
    [Header("Potion Settings")]
    public GameObject potionPrefab;
    public float spawnX = 12f;
    public float groundY = -3f;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 12f;
    public float maxSpawnInterval = 20f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float destroyX = -12f; // x-position where potions get destroyed

    private float timer;
    private float nextSpawnTime;

    void Start()
    {
        // Pick first random interval
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= nextSpawnTime)
        {
            SpawnPotion();
            timer = 0f;
            nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    void SpawnPotion()
    {
        Vector3 pos = new Vector3(spawnX, groundY, 0);
        GameObject potion = Instantiate(potionPrefab, pos, Quaternion.identity);

        // Add mover directly
        potion.AddComponent<EnergyPotionMover>().Init(moveSpeed, destroyX);
    }
}

public class EnergyPotionMover : MonoBehaviour
{
    private float speed;
    private float destroyX;

    public void Init(float moveSpeed, float destroyPosX)
    {
        speed = moveSpeed;
        destroyX = destroyPosX;
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}

