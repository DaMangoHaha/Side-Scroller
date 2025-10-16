using UnityEngine;

public class FinishFlagSpawner : MonoBehaviour
{
    public GameObject finishFlagPrefab;
    public Vector3 spawnPosition = new Vector3(12f, -2.5f, 0f);
    public float spawnDelay = 90f; // 1 minute 30 seconds

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnDelay)
        {
            Instantiate(finishFlagPrefab, spawnPosition, Quaternion.identity);
            enabled = false; // Stop spawning again
        }
    }
}
