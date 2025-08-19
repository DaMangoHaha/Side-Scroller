using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    public GameObject groundPrefab;   // assign prefab in Inspector
    public int numberOfTiles = 3;     // how many tiles visible at once
    public float tileWidth = 10f;     // width of each ground tile
    public float scrollSpeed = 5f;

    private GameObject[] groundTiles;
    private float leftEdge;

    void Start()
    {
        groundTiles = new GameObject[numberOfTiles];

        for (int i = 0; i < numberOfTiles; i++)
        {
            groundTiles[i] = Instantiate(groundPrefab, new Vector3(i * tileWidth, -3f, 0), Quaternion.identity);
        }

        leftEdge = -tileWidth; // reposition threshold
    }

    void Update()
    {
        foreach (GameObject tile in groundTiles)
        {
            tile.transform.position += Vector3.left * scrollSpeed * Time.deltaTime;

            // when tile goes off screen, move it to the right end
            if (tile.transform.position.x < leftEdge)
            {
                float rightMostX = FindRightMostTileX();
                tile.transform.position = new Vector3(rightMostX + tileWidth, tile.transform.position.y, 0);
            }
        }
    }

    float FindRightMostTileX()
    {
        float maxX = float.MinValue;
        foreach (GameObject tile in groundTiles)
        {
            if (tile.transform.position.x > maxX)
                maxX = tile.transform.position.x;
        }
        return maxX;
    }
}
