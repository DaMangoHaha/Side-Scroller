using UnityEngine;
using UnityEngine.SceneManagement; // for restarting or ending the game
using UnityEngine.UI; // if I add a UI bar later

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float depletionRate = 5f; // how fast energy drains per second

    [Header("UI")]
    public Slider energySlider; // optional Unity UI slider

    void Start()
    {
        currentEnergy = maxEnergy;

        if (energySlider != null)
            energySlider.maxValue = maxEnergy;
    }

    void Update()
    {
        // Deplete energy over time
        currentEnergy -= depletionRate * Time.deltaTime;

        // Clamp to 0 so it doesn’t go negative
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        // Update UI
        if (energySlider != null)
            energySlider.value = currentEnergy;

        // End game if empty
        if (currentEnergy <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("Energy depleted! Game Over.");
        // Example: Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

