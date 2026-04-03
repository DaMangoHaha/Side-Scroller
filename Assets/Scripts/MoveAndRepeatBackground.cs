using UnityEngine;

/// <summary>
/// Seamless parallax scrolling background.
/// Just attach this to a single background GameObject with a SpriteRenderer.
/// The script automatically creates a second copy and manages the looping.
/// No empty GameObjects, no manual positioning needed.
/// </summary>
public class MoveAndRepeatBackground : MonoBehaviour
{
    public float speed = 5f;

    private Transform[] segments;
    private float spriteWidth;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("MoveAndRepeatBackground: No SpriteRenderer found on " + gameObject.name, this);
            enabled = false;
            return;
        }

        spriteWidth = sr.bounds.size.x;

        // Create a duplicate placed directly to the right of this one
        GameObject copy = Instantiate(gameObject, transform.parent);
        copy.name = gameObject.name + "_Copy";

        // Remove the script from the copy so only this instance drives both
        Destroy(copy.GetComponent<MoveAndRepeatBackground>());

        Vector3 copyPos = transform.position;
        copyPos.x += spriteWidth;
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
            if (segments[i].position.x <= -spriteWidth)
            {
                Vector3 pos = segments[i].position;
                pos.x += spriteWidth * 2f;
                segments[i].position = pos;
            }
        }
    }
}
