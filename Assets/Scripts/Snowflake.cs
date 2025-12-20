using UnityEngine;

public class Snowflake : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float amplitude = 0.5f;  // wave height
    public float frequency = 3f;   // wave speed

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Drift left
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Sine wave float
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, 0);

        // Destroy if off screen
        if (transform.position.x < -12f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CrystalAbility ability = other.GetComponent<CrystalAbility>();
            if (ability != null)
                ability.CollectSnowflake();
            SoundManager.Instance.PlaySound2D("Snowflake");

            Destroy(gameObject);
        }
    }

}

