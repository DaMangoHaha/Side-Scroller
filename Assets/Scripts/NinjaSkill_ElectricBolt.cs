using UnityEngine;

public class NinjaSkill_ElectricBolt : MonoBehaviour
{
    [Header("Skill Settings")]
    public float cooldownTime = 30f;
    private float cooldownRemaining = 0f;

    [Header("Bolt Settings")]
    public GameObject electricBoltPrefab; // assign in Inspector
    public Transform boltSpawnPoint;      // where Bolt appears

    [Header("UI")]
    public GameObject readyIcon;          // like Bits/Thief indicators

    void Start()
    {
        cooldownRemaining = cooldownTime;
        if (readyIcon != null) readyIcon.SetActive(false);
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
                if (readyIcon != null) readyIcon.SetActive(true);
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

        Instantiate(electricBoltPrefab, boltSpawnPoint.position, Quaternion.identity);

        // reset cooldown
        cooldownRemaining = cooldownTime;
        if (readyIcon != null) readyIcon.SetActive(false);
    }
}
