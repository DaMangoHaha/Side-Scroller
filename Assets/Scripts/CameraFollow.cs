using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, target.position.x, smoothSpeed * Time.deltaTime);
        transform.position = pos;
    }
}

