using UnityEngine;
using System.Collections;

public class ChillTarget : MonoBehaviour
{
    public Sprite chilledSprite;
    public float driftSpeed = 5f;

    private bool isChilled = false;

    public void ApplyChill()
    {
        if (isChilled) return;
        isChilled = true;

        // Disable all other scripts except this one
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != this)
                s.enabled = false;
        }

        // Freeze physics if it exists
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Animator OFF so it stops overriding sprite
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.enabled = false;

        // Swap sprite to chilled
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = chilledSprite;

        // Disable collider so it's harmless
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Make chilled object drift left like an obstacle
        StartCoroutine(DriftLeft());
    }

    private IEnumerator DriftLeft()
    {
        while (transform.position.x > -15f)
        {
            transform.position += Vector3.left * driftSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}


