using UnityEngine;

// Centres the camera on the map and sizes it so the whole lab is visible at once, like
// a floor plan, adapting to whatever aspect ratio the game runs at. Used instead of
// follow-the-player so the player can see every room.
[RequireComponent(typeof(Camera))]
public class CameraFitMap : MonoBehaviour
{
    public Vector2 worldMin;
    public Vector2 worldMax;
    public float margin = 1f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        Vector2 center = (worldMin + worldMax) * 0.5f;
        transform.position = new Vector3(center.x, center.y, transform.position.z);

        float width = worldMax.x - worldMin.x;
        float height = worldMax.y - worldMin.y;
        float sizeForHeight = height * 0.5f;
        float sizeForWidth = width * 0.5f / Mathf.Max(0.01f, cam.aspect);
        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth) + margin;
    }
}
