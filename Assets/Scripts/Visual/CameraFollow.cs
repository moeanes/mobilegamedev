using UnityEngine;

// Smoothly follows a target and stays inside the arena bounds.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 6f;
    public Vector2 worldMin = new Vector2(-14f, -9f);
    public Vector2 worldMax = new Vector2(14f, 9f);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float clampedX = Mathf.Clamp(target.position.x, worldMin.x + halfWidth, worldMax.x - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, worldMin.y + halfHeight, worldMax.y - halfHeight);

        Vector3 desired = new Vector3(clampedX, clampedY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
