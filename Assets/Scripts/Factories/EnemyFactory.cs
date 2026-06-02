using UnityEngine;

// Builds the enemy and projectile templates in code. Templates start inactive; the
// LevelManager clones them with Instantiate and activates the clones.
public static class EnemyFactory
{
    public static GameObject CreatePlayerBulletTemplate()
    {
        GameObject root = new GameObject("PlayerBullet");
        root.SetActive(false);
        AddProjectileBody(root, 0.15f);
        AddProjectileVisual(root, "Projectiles/doctor_projectile", new Color(0.55f, 0.9f, 1f, 1f), 0.5f);
        root.AddComponent<Bullet>();
        return root;
    }

    public static GameObject CreateEnemyProjectileTemplate(string spriteResource)
    {
        GameObject root = new GameObject("EnemyProjectile");
        root.SetActive(false);
        AddProjectileBody(root, 0.15f);
        AddProjectileVisual(root, spriteResource, Color.white, 0.6f);
        root.AddComponent<EnemyProjectile>();
        return root;
    }

    public static GameObject CreateMeleeTemplate()
    {
        GameObject root = CreateEnemyBody("MeleeEnemyTemplate", "Characters/enemy1", 1.3f, 7);
        EnemyHealth health = root.AddComponent<EnemyHealth>();
        health.maxHealth = 3;

        MeleeEnemy melee = root.AddComponent<MeleeEnemy>();
        melee.moveSpeed = 2.4f;
        return root;
    }

    public static GameObject CreateRangedTemplate(string name, string sheetResource, int frameCount, int health, float moveSpeed, GameObject projectileTemplate)
    {
        GameObject root = CreateEnemyBody(name, sheetResource, 1.3f, frameCount);
        root.AddComponent<EnemyHealth>().maxHealth = health;

        RangedEnemy ranged = root.AddComponent<RangedEnemy>();
        ranged.projectilePrefab = projectileTemplate;
        ranged.moveSpeed = moveSpeed;
        return root;
    }

    private static GameObject CreateEnemyBody(string name, string sheetResource, float visualScale, int frameCount)
    {
        GameObject root = new GameObject(name);
        root.SetActive(false);
        root.layer = GameLayers.Enemy;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(visualScale, visualScale, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 10;

        Sprite[] frames = SpriteSheetLoader.Load(sheetResource, 32, 32);
        if (frames.Length > 0)
        {
            renderer.sprite = frames[0];
        }

        visual.AddComponent<SimpleSpriteAnimator>();
        EnemyVisual enemyVisual = visual.AddComponent<EnemyVisual>();
        enemyVisual.sheetResource = sheetResource;
        enemyVisual.frameCount = frameCount;

        return root;
    }

    private static void AddProjectileBody(GameObject root, float radius)
    {
        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = radius;
    }

    private static void AddProjectileVisual(GameObject root, string spriteResource, Color tint, float scale)
    {
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteSheetLoader.LoadSingle(spriteResource, 32, 32, 0);
        renderer.color = tint;
        renderer.sortingOrder = 15;
    }
}
