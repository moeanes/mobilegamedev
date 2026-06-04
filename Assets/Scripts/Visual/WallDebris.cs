using UnityEngine;

// One broken wall chunk: flies outward, spins, falls under gravity and fades, then removes
// itself. Spawned in a burst when the boss smashes the walls, so the arena visibly breaks
// apart instead of the walls just blinking out.
public class WallDebris : MonoBehaviour
{
    private Vector2 velocity;
    private float angularVelocity;
    private float life;
    private float maxLife = 1f;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    public void Launch(Vector2 initialVelocity, float spinDegreesPerSecond, float lifeSeconds)
    {
        velocity = initialVelocity;
        angularVelocity = spinDegreesPerSecond;
        maxLife = Mathf.Max(0.05f, lifeSeconds);
        life = maxLife;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        velocity += Vector2.down * 12f * Time.deltaTime; // gravity drags the chunks down
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);

        float fraction = life / maxLife;
        transform.localScale = baseScale * Mathf.Lerp(0.15f, 1f, fraction);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = fraction;
            spriteRenderer.color = color;
        }
    }
}
