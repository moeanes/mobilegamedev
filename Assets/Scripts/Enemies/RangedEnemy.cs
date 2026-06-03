using UnityEngine;

// Keeps its distance from the player and fires projectiles at it.
[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float preferredDistance = 5.5f;
    public float fireRange = 8f;
    public float shotsPerSecond = 0.7f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 7f;
    public int projectileDamage = 1;

    private const float Deadzone = 0.6f;
    private Rigidbody2D body;
    private Transform target;
    private float radius = 0.4f;
    private float cooldown;

    public void SetTarget(Transform player)
    {
        target = player;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;

        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            radius = circle.radius;
        }
    }

    private void FixedUpdate()
    {
        if (target == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
        {
            return;
        }

        float distance = Vector2.Distance(body.position, target.position);
        Vector2 direction = ((Vector2)target.position - body.position).normalized;
        Vector2 move = Vector2.zero;

        if (distance > preferredDistance + Deadzone)
        {
            move = direction;       // close the gap
        }
        else if (distance < preferredDistance - Deadzone)
        {
            move = -direction;      // back away to keep range
        }

        if (move.sqrMagnitude > 0.0001f)
        {
            move = EnemyNavigation.Steer(body.position, move, radius);
        }

        body.MovePosition(body.position + move * moveSpeed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (target == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
        {
            return;
        }

        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }

        if (cooldown <= 0f && Vector2.Distance(transform.position, target.position) <= fireRange)
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        cooldown = 1f / Mathf.Max(0.1f, shotsPerSecond);

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        GameObject projectileObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectileObject.SetActive(true);

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.speed = projectileSpeed;
            projectile.damage = projectileDamage;
            projectile.Launch(direction);
        }
    }
}
