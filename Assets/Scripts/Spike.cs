using UnityEngine;

public class Spike : MonoBehaviour
{
    public float damage = 20f;   // how much energy it removes
    public float speed = 5f;     // scroll speed

    void Update()
    {
        // Move left
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Destroy when off-screen
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerEnergy energy = collision.gameObject.GetComponent<PlayerEnergy>();
            if (energy != null)
            {
                energy.currentEnergy = Mathf.Clamp(
                    energy.currentEnergy - damage,
                    0, energy.maxEnergy
                );
            }

            // OPTIONAL: Add knockback or hurt animation here
            Debug.Log("Player hit a spike! -" + damage + " Energy");
        }
    }
}


