using UnityEngine;

public class BushSpawner : MonoBehaviour
{
    [System.Serializable]
    public class BushLayer
    {
        public string name;
        public GameObject bushPrefab; // Capsule or sprite prefab
        public float spawnInterval = 4f;
        public float moveSpeed = 2f;
        public float spawnYMin = 0f;
        public float spawnYMax = 4f;
        public float destroyX = -12f;
        public float spawnX = 12f;
        public Vector2 scaleRange = new Vector2(0.8f, 1.3f);
        [HideInInspector] public float timer = 0f;
    }

    [Header("Bush Layers")]
    public BushLayer[] bushLayers;

    void Update()
    {
        foreach (BushLayer layer in bushLayers)
        {
            layer.timer += Time.deltaTime;
            if (layer.timer >= layer.spawnInterval)
            {
                SpawnBush(layer);
                layer.timer = 0f;
            }
        }
    }

    void SpawnBush(BushLayer layer)
    {
        if (layer.bushPrefab == null) return;

        Vector3 spawnPos = new Vector3(
            layer.spawnX,
            Random.Range(layer.spawnYMin, layer.spawnYMax),
            0
        );

        GameObject bush = Instantiate(layer.bushPrefab, spawnPos, Quaternion.identity);
        bush.transform.Rotate(0f, 0f, 0f); // force rotate in world space


        // Ensure bushes render behind coins
        SpriteRenderer sr = bush.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = "Ground";
        spawnPos.z = -1f; // further up



        // Slight random scale variation
        float scale = Random.Range(layer.scaleRange.x, layer.scaleRange.y);
        bush.transform.localScale = new Vector3(scale, scale, 1f);

        // Assign a random speed variance for a natural look
        float actualSpeed = layer.moveSpeed + Random.Range(-0.5f, 0.5f);

        // Add movement logic
        CloudMover mover = bush.AddComponent<CloudMover>();
        mover.Init(actualSpeed, layer.destroyX);
    }
}
