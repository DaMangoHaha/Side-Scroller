using UnityEngine;

public class SnowflakeSpawner : MonoBehaviour
{
    public GameObject snowflakePrefab;
    public float spawnInterval = 4f;
    public float minY = -2f;
    public float maxY = 3f;

    private float timer;

    void Update()
    {
        // Only spawn if Crystal is the selected character (from save system)
        SaveData data = SaveSystem.LoadData();
        if (data.equippedCharacter != "Crystal")
            return;

        // Pause spawning while Crystal is cursed
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            CrystalAbility crystalAbility = player.GetComponent<CrystalAbility>();
            if (crystalAbility != null && crystalAbility.isCursedPaused)
                return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnSnowflake();
        }
    }

    void SpawnSnowflake()
    {
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(12f, y, 0);

        Instantiate(snowflakePrefab, pos, Quaternion.identity);
    }
}

