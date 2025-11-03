using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [System.Serializable]
    public class CloudLayer
    {
        public string name;
        public GameObject cloudPrefab; // Capsule or sprite prefab
        public float spawnInterval = 4f;
        public float moveSpeed = 2f;
        public float spawnYMin = 0f;
        public float spawnYMax = 4f;
        public float destroyX = -12f;
        public float spawnX = 12f;
        public Vector2 scaleRange = new Vector2(0.8f, 1.3f);
        [HideInInspector] public float timer = 0f;
    }

    [Header("Cloud Layers")]
    public CloudLayer[] cloudLayers;

    void Update()
    {
        foreach (CloudLayer layer in cloudLayers)
        {
            layer.timer += Time.deltaTime;
            if (layer.timer >= layer.spawnInterval)
            {
                SpawnCloud(layer);
                layer.timer = 0f;
            }
        }
    }

    void SpawnCloud(CloudLayer layer)
    {
        if (layer.cloudPrefab == null) return;

        Vector3 spawnPos = new Vector3(
            layer.spawnX,
            Random.Range(layer.spawnYMin, layer.spawnYMax),
            0
        );

        GameObject cloud = Instantiate(layer.cloudPrefab, spawnPos, Quaternion.identity);
        cloud.transform.Rotate(0f, 0f, 90f); // force rotate in world space


        // Ensure clouds render behind coins
        SpriteRenderer sr = cloud.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = "BackgroundClouds";
        spawnPos.z = 5f; // further into the background



        // Slight random scale variation
        float scale = Random.Range(layer.scaleRange.x, layer.scaleRange.y);
        cloud.transform.localScale = new Vector3(scale, scale, 1f);

        // Assign a random speed variance for a natural look
        float actualSpeed = layer.moveSpeed + Random.Range(-0.5f, 0.5f);

        // Add movement logic
        CloudMover mover = cloud.AddComponent<CloudMover>();
        mover.Init(actualSpeed, layer.destroyX);
    }
}