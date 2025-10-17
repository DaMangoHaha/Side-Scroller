using UnityEngine;

public class FinishFlagMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float destroyX = -12f;

    private void Update()
    {
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime); // Moves the flag to the left every frame
            if (transform.position.x < destroyX)
            {
                Destroy(gameObject);
            }
        }
    }
}
