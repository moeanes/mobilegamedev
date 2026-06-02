using UnityEngine;

// Builds the doctor (player) GameObject entirely in code.
public static class PlayerFactory
{
    public static GameObject Create(Vector3 spawn, Vector2 arenaMin, Vector2 arenaMax, GameObject bulletTemplate)
    {
        GameObject root = new GameObject("Player");
        root.transform.position = spawn;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        PlayerMovement movement = root.AddComponent<PlayerMovement>();
        movement.arenaMin = arenaMin;
        movement.arenaMax = arenaMax;

        PlayerHealth health = root.AddComponent<PlayerHealth>();
        health.maxHealth = 5;
        health.HealthChanged += (current, max) => GameManager.Instance?.OnPlayerHealthChanged(current, max);

        PlayerShooter shooter = root.AddComponent<PlayerShooter>();
        shooter.bulletPrefab = bulletTemplate;
        shooter.firePoint = root.transform;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 20;

        Sprite[] idleFrames = SpriteSheetLoader.Load("Characters/doctor_idle", 32, 32);
        if (idleFrames.Length > 0)
        {
            renderer.sprite = idleFrames[0];
        }

        visual.AddComponent<SimpleSpriteAnimator>();
        visual.AddComponent<PlayerVisual>();

        return root;
    }
}
