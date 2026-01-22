using UnityEngine;
using System.Collections;

public class CubitPassive : MonoBehaviour
{
    [Header("Override: Delete Settings")]
    public float cooldown = 15f;
    public float fallVelocityThreshold = -2f;

    private bool abilityReady = true;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!abilityReady)
            return;

        // Must be falling downward fast enough
        if (rb.linearVelocity.y > fallVelocityThreshold)
            return;

        // Tag check instead of layers
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            ExecuteOverride(collision.gameObject);
        }
    }

    void ExecuteOverride(GameObject target)
    {
        abilityReady = false;

        Destroy(target);

        StartCoroutine(CooldownRoutine());
    }


    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        abilityReady = true;
    }
}
