using UnityEngine;

public class GlaciateArea : MonoBehaviour
{
    public CircleCollider2D detectionCollider;

    void Awake()
    {
        detectionCollider = GetComponent<CircleCollider2D>();
        detectionCollider.enabled = false;
    }

    public void EnableRadius(bool enable)
    {
        detectionCollider.enabled = enable;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If enemy or obstacle, apply “Chill”
        ChillTarget chill = other.GetComponent<ChillTarget>();
        if (chill != null)
            chill.ApplyChill();
    }
}

