using UnityEngine;

public class ChillTarget : MonoBehaviour
{
    public Sprite chilledSprite;
    public bool chilled = false;

    public void ApplyChill()
    {
        if (chilled) return;

        chilled = true;

        // Replace sprite with icy version
        GetComponent<SpriteRenderer>().sprite = chilledSprite;

        // Disable collisions + movement
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        SlimeBase slime = GetComponent<SlimeBase>();
        if (slime != null)
            slime.enabled = false;

        // Object becomes harmless but still visible
    }
}

