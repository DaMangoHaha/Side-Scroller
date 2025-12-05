using UnityEngine;
using System.Collections;

public class WizKidSkill : MonoBehaviour
{
    [Header("Settings")]
    public float cooldown = 30f;
    public float tickInterval = 0.5f;

    [Header("Healing Amounts")]
    public float smallHeal = 1.5f;
    public float mediumHeal = 3f;
    public float largeHeal = 6f;

    [Header("Durations")]
    public float smallDuration = 3f;
    public float mediumDuration = 5f;
    public float largeDuration = 7f;

    [Header("Effects")]
    public GameObject confettiPrefab;

    private PlayerEnergy playerEnergy;
    private float timer = 0f;
    private bool abilityReady = false;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        timer = cooldown;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(ActivateSproutingSorcery());
            timer = cooldown;
        }
    }

    private IEnumerator ActivateSproutingSorcery()
    {
        // Pick one of three effects
        int choice = Random.Range(0, 3);

        float duration;
        float healAmount;

        switch (choice)
        {
            default:
            case 0: duration = smallDuration; healAmount = smallHeal; break;
            case 1: duration = mediumDuration; healAmount = mediumHeal; break;
            case 2: duration = largeDuration; healAmount = largeHeal; break;
        }

        // Spawn the confetti
        StartCoroutine(SpawnConfetti(duration));

        float timePassed = 0f;

        while (timePassed < duration)
        {
            playerEnergy.RestoreEnergy(healAmount);
            timePassed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
    }

    private IEnumerator SpawnConfetti(float duration)
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            if (confettiPrefab != null)
            {
                // spawn around the player within a small radius
                Vector3 offset = Random.insideUnitCircle * 1f;
                Instantiate(confettiPrefab, transform.position + offset, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.1f); // rapid burst effect
        }
    }
}
