using UnityEngine;

// The doctor's projectile. Flies straight, damages the first enemy it touches.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public float lifeTime = 2.5f;

    private Rigidbody2D body;
    private GameObject owner;
    private bool consumed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    public void Launch(Vector2 direction, GameObject shooter)
    {
        owner = shooter;
        Vector2 aim = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        body.linearVelocity = aim * speed;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed || other.gameObject == owner || other.GetComponent<Bullet>() != null)
        {
            return;
        }

        if (other.gameObject.layer == GameLayers.Wall)
        {
            consumed = true;
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            consumed = true;
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
