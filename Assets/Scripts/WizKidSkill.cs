using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI")]
    public Image wizIcon;
    private Color inactiveColor;
    private Color activeColor;

    [Header("Effects")]
    public GameObject confettiPrefab;

    private PlayerEnergy playerEnergy;
    private float timer = 0f;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        timer = cooldown;
        if (wizIcon != null)
        {
            activeColor = wizIcon.color;
            inactiveColor = wizIcon.color;
            inactiveColor.a = 0.2f; // faded look
            wizIcon.color = inactiveColor;
        }

    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(ActivateSproutingSorcery());
            timer = cooldown;
            if (wizIcon != null)
                wizIcon.color = activeColor;
        }
    }

    private IEnumerator ActivateSproutingSorcery()
    {
        // Pick one of three effects
        int choice = Random.Range(0, 3);

        float duration;
        float healAmount;

        // Assign duration and healAmount based on choice
        switch (choice)
        {
            case 0:
                duration = smallDuration;
                healAmount = smallHeal;
                SoundManager.Instance.PlaySound2D("SmallBurst");
                break;
            case 1:
                duration = mediumDuration;
                healAmount = mediumHeal;
                SoundManager.Instance.PlaySound2D("MediumBurst");
                break;
            case 2:
                duration = largeDuration;
                healAmount = largeHeal;
                SoundManager.Instance.PlaySound2D("LargeBurst");
                break;
            default:
                duration = smallDuration;
                healAmount = smallHeal;
                break;
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
        if (wizIcon != null)
            wizIcon.color = inactiveColor;
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
