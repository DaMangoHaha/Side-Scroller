using UnityEngine;
using System.Collections;

public class CrystalAbility : MonoBehaviour
{
    public int snowflakesNeeded = 5;
    private int currentSnowflakes = 0;

    public bool abilityReady = false;
    public float glaciateDuration = 5f;
    public GameObject glaciateEffectPrefab;

    private bool abilityActive = false;

    void Update()
    {
        // Activate ability with SHIFT
        if (abilityReady && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ActivateGlaciate());
        }
    }

    public void CollectSnowflake()
    {
        if (abilityActive) return;

        currentSnowflakes++;

        if (currentSnowflakes >= snowflakesNeeded)
        {
            abilityReady = true;
            // TODO: Change skill icon to READY state
        }
    }

    private IEnumerator ActivateGlaciate()
    {
        abilityActive = true;
        abilityReady = false;
        currentSnowflakes = 0;

        // Spawn mist visual
        GameObject effect = Instantiate(glaciateEffectPrefab, transform.position, Quaternion.identity, transform);

        // Turn on hitbox
        GlaciateArea glaciate = GetComponentInChildren<GlaciateArea>();
        glaciate.EnableRadius(true);

        yield return new WaitForSeconds(glaciateDuration);

        glaciate.EnableRadius(false);
        Destroy(effect);
        abilityActive = false;
    }
}

