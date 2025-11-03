using UnityEngine;

public class CloudMover : MonoBehaviour
{
    private float speed;
    private float destroyX;

    public void Init(float moveSpeed, float destroyPosX)
    {
        speed = moveSpeed;
        destroyX = destroyPosX;
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);


        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
