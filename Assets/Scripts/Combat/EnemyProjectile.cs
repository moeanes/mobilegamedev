using UnityEngine;

// Fired by ranged enemies. Damages the player on contact.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 1;
    public float lifeTime = 3f;

    private Rigidbody2D body;
    private bool consumed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    public void Launch(Vector2 direction)
    {
        Vector2 aim = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        body.linearVelocity = aim * speed;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed)
        {
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            consumed = true;
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
