using System.Collections;
using UnityEngine;

public class CubitPassive : MonoBehaviour
{
    [Header("Override: Delete Settings")]
    public float cooldown = 15f;
    public LayerMask stompableLayers;
    public float minFallSpeed = -1f;
    private bool abilityReady = true;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!abilityReady)
            return;

        // Check if Cubit is falling
        if (rb.linearVelocity.y > minFallSpeed)
            return;

        //Check if collision came from above
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                ExecuteOverride(collision.gameObject);
                break;
            }
        }
    }

    void ExecuteOverride(GameObject target)
    {
        abilityReady = false;

        //Kill enemy / obstacle
        Destroy(target);

        //Skill Dialogue
        // *Put code here*
        
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        abilityReady = true;
    }
}

