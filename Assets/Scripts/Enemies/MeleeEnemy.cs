using UnityEngine;

// Walks straight at the player and deals contact damage on a cooldown.
[RequireComponent(typeof(Rigidbody2D))]
public class MeleeEnemy : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public int contactDamage = 1;
    public float damageCooldown = 0.8f;

    private Rigidbody2D body;
    private Transform target;
    private float lastDamageTime = -999f;

    public void SetTarget(Transform player)
    {
        target = player;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (target == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
        {
            return;
        }

        Vector2 toTarget = (Vector2)target.position - body.position;
        if (toTarget.sqrMagnitude < 0.0004f)
        {
            return;
        }

        body.MovePosition(body.position + toTarget.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
            lastDamageTime = Time.time;
        }
    }
}
