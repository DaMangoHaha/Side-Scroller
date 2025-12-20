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

    void Start()
    {
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
            SoundManager.Instance.PlaySound2D("ElectricBolt");
        }
    }

    void FireElectricBolt()
    {
        if (electricBoltPrefab == null || boltSpawnPoint == null)
        {
            Debug.LogError("Electric bolt or spawn point missing!");
            return;
        }

        Instantiate(electricBoltPrefab, boltSpawnPoint.position, Quaternion.identity);

        // reset cooldown
        cooldownRemaining = cooldownTime;
        if (readyIcon != null)
            readyIcon.color = inactiveColor;
    }
}
