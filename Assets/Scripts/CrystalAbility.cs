using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CrystalAbility : MonoBehaviour
{
    public int snowflakesNeeded = 5;
    private int currentSnowflakes = 0;

    public bool abilityReady = false;
    public float glaciateDuration = 5f;
    public GameObject glaciateEffectPrefab;

    [Header("Skill Icon")]
    public Image skillIcon;
    public Color fadedColor;
    public Color readyColor;
    public float flickerSpeed = 6f; // how fast icon flickers


    private bool abilityActive = false;
    private PlayerEnergy playerEnergy;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();

        if (skillIcon != null)
        {
            readyColor = skillIcon.color;     // normal visible sprite
            fadedColor = skillIcon.color;
            fadedColor.a = 0.2f;              // faded power-down
            skillIcon.color = fadedColor;
        }
    }

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

            if (skillIcon != null)
                skillIcon.color = readyColor;
        }
    }


    private IEnumerator ActivateGlaciate()
    {
        abilityActive = true;
        abilityReady = false;
        currentSnowflakes = 0;

        // Start flicker
        if (skillIcon != null)
            StartCoroutine(FlickerIcon());

        // Spawn mist effect
        GameObject effect = Instantiate(glaciateEffectPrefab, transform.position, Quaternion.identity, transform);

        GlaciateArea glaciate = GetComponentInChildren<GlaciateArea>();
        glaciate.EnableRadius(true);

        yield return new WaitForSeconds(glaciateDuration);

        glaciate.EnableRadius(false);
        Destroy(effect);

        abilityActive = false;

        // End flicker ? return to faded
        if (skillIcon != null)
            skillIcon.color = fadedColor;
    }

    private IEnumerator FlickerIcon()
    {
        float t = 0;

        while (abilityActive)
        {
            t += Time.deltaTime * flickerSpeed;

            float alpha = Mathf.Abs(Mathf.Sin(t));   // goes 0 ? 1 ? 0 smoothly
            Color c = readyColor;
            c.a = alpha;

            if (skillIcon != null)
                skillIcon.color = c;

            yield return null;
        }
    }
}

