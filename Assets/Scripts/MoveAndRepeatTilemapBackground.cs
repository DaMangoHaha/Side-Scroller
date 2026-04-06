using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Seamless parallax scrolling background for Tilemaps.
/// Just attach this to a single GameObject that has a Tilemap + TilemapRenderer.
/// The script automatically creates a second copy and manages the looping.
/// No empty GameObjects, no manual positioning needed.
/// </summary>
public class MoveAndRepeatTilemapBackground : MonoBehaviour
{
    public float speed = 5f;

    private Transform[] segments;
    private float tilemapWidth;

    void Start()
    {
        TilemapRenderer tr = GetComponent<TilemapRenderer>();
        Tilemap tilemap = GetComponent<Tilemap>();

        if (tr == null || tilemap == null)
        {
            Debug.LogError("MoveAndRepeatTilemapBackground: No Tilemap or TilemapRenderer found on " + gameObject.name, this);
            enabled = false;
            return;
        }

        // Compress the bounds to only include painted tiles, then get the width
        tilemap.CompressBounds();
        tilemapWidth = tilemap.localBounds.size.x * transform.lossyScale.x;

        // Create a duplicate placed directly to the right of this one
        GameObject copy = Instantiate(gameObject, transform.parent);
        copy.name = gameObject.name + "_Copy";

        // Remove the script from the copy so only this instance drives both
        Destroy(copy.GetComponent<MoveAndRepeatTilemapBackground>());

        Vector3 copyPos = transform.position;
        copyPos.x += tilemapWidth;
        copy.transform.position = copyPos;

        // Track both segments
        segments = new Transform[] { transform, copy.transform };
    }

    void Update()
    {
        float movement = speed * Time.deltaTime;

        for (int i = 0; i < segments.Length; i++)
        {
            // Move left
            segments[i].Translate(Vector3.left * movement);

            // When a segment has scrolled one full width past its starting side, wrap it forward
            if (segments[i].position.x <= -tilemapWidth)
            {
                Vector3 pos = segments[i].position;
                pos.x += tilemapWidth * 2f;
                segments[i].position = pos;
            }
        }
    }
}
