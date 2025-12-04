using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public float moveSpeed = 5f;

    private bool activated = false;

    void Update()
    {
        if (activated)
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }

    [System.Obsolete]
    public void ActivateTrigger()
    {
        activated = true;

        // Disable all spawners
        foreach (var spawner in FindObjectsOfType<MonoBehaviour>())
        {
            if (spawner is SlimeSpawner || spawner is CoinSpawner ||
                spawner is EnergyPotionSpawner || spawner.name.Contains("Spawner"))
            {
                spawner.enabled = false;
            }
        }
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //  if (other.CompareTag("Player"))
    //  {
    //     CutsceneController.Instance.StartCutscene();
    // }
    //  }
}

