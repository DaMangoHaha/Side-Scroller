using UnityEngine;

/// <summary>
/// Unified spawner for all Level 6 magic traps.
/// Randomly selects which bolt type to spawn based on configurable weights.
/// Place this on an empty GameObject in the Level 6 scene.
///
/// Prefab requirements:
///   - Magic Bolt         — SpriteRenderer, Collider2D, Rigidbody2D (Kinematic), MagicBolt script
///   - Charged Magic Bolt — SpriteRenderer, Collider2D, Rigidbody2D (Kinematic), ChargedMagicBolt script
///   - Twin Magic Bolt    — SpriteRenderer, Collider2D, Rigidbody2D (Kinematic), TwinMagicBolt script
///   - Spark Bolt         — SpriteRenderer, Collider2D, Rigidbody2D (Kinematic), SparkBolt script
/// </summary>
public class MagicBoltSpawner : MonoBehaviour
{
    [Header("Prefabs — Assign in Inspector")]
    [Tooltip("Standard Magic Bolt prefab (has MagicBolt script)")]
    public GameObject magicBoltPrefab;

    [Tooltip("Charged Magic Bolt prefab (has ChargedMagicBolt script)")]
    public GameObject chargedMagicBoltPrefab;

    [Tooltip("Twin Magic Bolt prefab (has TwinMagicBolt script)")]
    public GameObject twinMagicBoltPrefab;

    [Tooltip("Spark Bolt prefab (has SparkBolt script)")]
    public GameObject sparkBoltPrefab;

    [Header("Spawn Position")]
    [Tooltip("X position to spawn bolts (off-screen right)")]
    public float spawnX = 12f;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;

    [Header("Spawn Weights (higher = more likely)")]
    [Tooltip("Relative weight for Magic Bolt")]
    public float weightMagicBolt = 1f;

    [Tooltip("Relative weight for Charged Magic Bolt")]
    public float weightChargedMagicBolt = 1f;

    [Tooltip("Relative weight for Twin Magic Bolt")]
    public float weightTwinMagicBolt = 1f;

    [Tooltip("Relative weight for Spark Bolt")]
    public float weightSparkBolt = 1f;

    // Internal
    private float timer;
    private float nextSpawnTime;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnRandomBolt();
            timer = 0f;
            ScheduleNextSpawn();
        }
    }

    private void ScheduleNextSpawn()
    {
        float baseInterval = Random.Range(minSpawnInterval, maxSpawnInterval);

        if (DifficultyManager.Instance != null)
            baseInterval *= DifficultyManager.Instance.spawnRateModifier;

        nextSpawnTime = baseInterval;
    }

    private void SpawnRandomBolt()
    {
        float totalWeight = weightMagicBolt + weightChargedMagicBolt +
                            weightTwinMagicBolt + weightSparkBolt;

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("MagicBoltSpawner: All weights are 0 — nothing to spawn.");
            return;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        // Magic Bolt
        cumulative += weightMagicBolt;
        if (roll < cumulative)
        {
            SpawnMagicBolt();
            return;
        }

        // Charged Magic Bolt
        cumulative += weightChargedMagicBolt;
        if (roll < cumulative)
        {
            SpawnChargedMagicBolt();
            return;
        }

        // Twin Magic Bolt
        cumulative += weightTwinMagicBolt;
        if (roll < cumulative)
        {
            SpawnTwinMagicBolt();
            return;
        }

        // Spark Bolt (fallthrough)
        SpawnSparkBolt();
    }

    // ??????????????????????????? Individual Spawn Methods ???????????????????????????

    private void SpawnMagicBolt()
    {
        if (magicBoltPrefab == null) return;

        // Y is randomised inside the MagicBolt script itself
        Vector3 pos = new Vector3(spawnX, 0f, 0f);
        Instantiate(magicBoltPrefab, pos, Quaternion.identity);
    }

    private void SpawnChargedMagicBolt()
    {
        if (chargedMagicBoltPrefab == null) return;

        // Y is randomised inside the ChargedMagicBolt script itself
        Vector3 pos = new Vector3(spawnX, 0f, 0f);
        Instantiate(chargedMagicBoltPrefab, pos, Quaternion.identity);
    }

    private void SpawnTwinMagicBolt()
    {
        if (twinMagicBoltPrefab == null) return;

        // Y is randomised inside the TwinMagicBolt script itself
        Vector3 pos = new Vector3(spawnX, 0f, 0f);
        Instantiate(twinMagicBoltPrefab, pos, Quaternion.identity);
    }

    private void SpawnSparkBolt()
    {
        if (sparkBoltPrefab == null) return;

        // Y is randomised inside the SparkBolt script itself
        Vector3 pos = new Vector3(spawnX, 0f, 0f);
        Instantiate(sparkBoltPrefab, pos, Quaternion.identity);
    }
}
