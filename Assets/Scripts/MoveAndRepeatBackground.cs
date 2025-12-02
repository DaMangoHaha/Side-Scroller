using UnityEngine;

public class MoveAndRepeatBackground : MonoBehaviour
{
    public float speed = 5f;           // Speed at which the background moves left
    public float resetPositionX = -20f; // X position where the background resets
    public float startPositionX = 20f;  // X position to move back to

    void Update()
    {
        // Move the background left
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Check if it has moved past the reset position
        if (transform.position.x <= resetPositionX)
        {
            Vector3 newPos = new Vector3(startPositionX, transform.position.y, transform.position.z);
            transform.position = newPos;
        }
    }
}
