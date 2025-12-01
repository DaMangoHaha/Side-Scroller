using UnityEngine;
using UnityEngine.UI;

public class NinjaSkill_ElectricBolt : MonoBehaviour
{
    [Header("Skill Settings")]
    public float cooldownTime = 30f;
    private float cooldownRemaining = 0f;

    [Header("Bolt Settings")]
    public GameObject electricBoltPrefab; // assign in Inspector
    public Transform boltSpawnPoint;      // where Bolt appears

    [Header("UI")]
    public Image readyIcon;          // like Bits/Thief indicators
    private Color inactiveColor;
    private Color activeColor;

    [Header("Audio")]
    public AudioClip boltActivateSFX;   // SFX when bolt is fired
    public AudioClip boltConsumeSFX;    // SFX when bolt destroys an object/enemy
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        cooldownRemaining = cooldownTime;
        if (readyIcon != null)
        {
            activeColor = readyIcon.color;
            inactiveColor = readyIcon.color;
            inactiveColor.a = 0.2f; // faded look
            readyIcon.color = inactiveColor;
        }
    }

    void Update()
    {
        // Update cooldown
        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0)
            {
                cooldownRemaining = 0;
                if (readyIcon != null)
                    readyIcon.color = activeColor;
            }
        }

        // Activate with Shift
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownRemaining == 0)
        {
            FireElectricBolt();
        }
    }

    void FireElectricBolt()
    {
        if (electricBoltPrefab == null || boltSpawnPoint == null)
        {
            Debug.LogError("Electric bolt or spawn point missing!");
            return;
        }
        if (audioSource != null && boltActivateSFX != null)
            audioSource.PlayOneShot(boltActivateSFX);

        Instantiate(electricBoltPrefab, boltSpawnPoint.position, Quaternion.identity);

        // reset cooldown
        cooldownRemaining = cooldownTime;
        if (readyIcon != null)
            readyIcon.color = inactiveColor;
    }
}
