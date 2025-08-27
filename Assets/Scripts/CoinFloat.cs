using UnityEngine;

public class CoinFloat : MonoBehaviour
{
    public float floatAmplitude = 1f; // how high it moves
    public float floatFrequency = 3f;   // how fast it oscillates

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, startPos.y + yOffset, transform.position.z);
    }
}

