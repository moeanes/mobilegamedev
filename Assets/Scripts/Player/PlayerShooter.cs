using UnityEngine;

// Fires a bullet toward the mouse on left click, capped by a fire rate. Legacy Input.
public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shotsPerSecond = 5f;
    public float bulletSpeed = 12f;
    public int bulletDamage = 1;
    public float muzzleOffset = 0.5f;

    private Camera mainCamera;
    private float cooldown;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }

        if (Input.GetMouseButton(0) && cooldown <= 0f)
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        cooldown = 1f / Mathf.Max(0.1f, shotsPerSecond);

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = AimDirection(origin);
        Vector3 muzzle = origin + (Vector3)(direction * muzzleOffset);

        GameObject bulletObject = Instantiate(bulletPrefab, muzzle, Quaternion.identity);
        bulletObject.SetActive(true);

        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed = bulletSpeed;
            bullet.damage = bulletDamage;
            bullet.Launch(direction, gameObject);
        }
    }

    private Vector2 AimDirection(Vector3 origin)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return Vector2.right;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)(mouseWorld - origin);
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
    }
}
