using UnityEngine;

public class Scroller : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Destroy when off-screen (so we don’t fill memory)
        if (transform.position.x < -15f) // adjust depending on your camera
        {
            Destroy(gameObject);
        }
    }
}

