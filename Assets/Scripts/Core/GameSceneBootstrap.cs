using UnityEngine;

// Sits in the (otherwise empty) Game scene and builds everything in code when it loads:
// camera, floor, GameManager, player, enemy templates, level manager, HUD.
// Which level it builds comes from LevelDatabase.PendingLevel.
[DefaultExecutionOrder(-100)]
public class GameSceneBootstrap : MonoBehaviour
{
    public Vector3 playerSpawn = Vector3.zero;
    public Vector2 arenaMin = new Vector2(-13f, -8f);
    public Vector2 arenaMax = new Vector2(13f, 8f);
    public float cameraSize = 6.5f;

    private void Awake()
    {
        LevelData data = LevelDatabase.Get(LevelDatabase.PendingLevel);

        ConfigureCollisions();
        EnsureGameManager();
        Camera camera = EnsureCamera();
        BuildFloor(data);
        BuildWalls();
        MapDecorator.Decorate(arenaMin, arenaMax, 10);

        GameObject bulletTemplate = EnemyFactory.CreatePlayerBulletTemplate();

        // Keep the player a little inside the walls so it never overlaps them.
        Vector2 playerMin = arenaMin + new Vector2(0.7f, 0.7f);
        Vector2 playerMax = arenaMax - new Vector2(0.7f, 0.7f);
        GameObject player = PlayerFactory.Create(playerSpawn, playerMin, playerMax, bulletTemplate);

        GameObject blueProjectile = EnemyFactory.CreateEnemyProjectileTemplate("Projectiles/enemy3_projectile");
        GameObject pinkProjectile = EnemyFactory.CreateEnemyProjectileTemplate("Projectiles/enemy_projectile");

        GameObject meleeTemplate = EnemyFactory.CreateMeleeTemplate();
        GameObject blueRanged = EnemyFactory.CreateRangedTemplate("BlueRanged", "Characters/enemy3", 5, 2, 2f, blueProjectile);
        GameObject pinkRanged = EnemyFactory.CreateRangedTemplate("PinkRanged", "Characters/enemy2", 1, 2, 1f, pinkProjectile);

        BuildLevelManager(data, player.transform, meleeTemplate, new[] { blueRanged, pinkRanged });
        Hud.Create();
        ConnectCamera(camera, player.transform);
    }

    private static void ConfigureCollisions()
    {
        // Enemies don't push each other (no clumping) and pass through furniture
        // (so they never get stuck). Walls and the player still block them.
        Physics2D.IgnoreLayerCollision(GameLayers.Enemy, GameLayers.Enemy, true);
        Physics2D.IgnoreLayerCollision(GameLayers.Enemy, GameLayers.Prop, true);
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }
    }

    private Camera EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = cameraSize;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);

        Vector3 position = camera.transform.position;
        camera.transform.position = new Vector3(position.x, position.y, -10f);
        return camera;
    }

    private void BuildFloor(LevelData data)
    {
        GameObject floor = new GameObject("Floor");
        floor.transform.position = new Vector3(0f, 0f, 5f);

        SpriteRenderer renderer = floor.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = -10;
        renderer.color = data.floorTint;
        renderer.sprite = CreateFloorTile();
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(arenaMax.x - arenaMin.x + 6f, arenaMax.y - arenaMin.y + 6f);
    }

    // A solid wall border around the arena so the level reads as an enclosed room,
    // not an open field. Walls block the player and enemies.
    private void BuildWalls()
    {
        Sprite wallTile = CreateWallTile();
        float width = arenaMax.x - arenaMin.x;
        float height = arenaMax.y - arenaMin.y;
        const float thickness = 1.2f;

        // Each wall sits just outside the arena edge, extending outward.
        MakeWall(wallTile, new Vector2(0f, arenaMax.y + thickness * 0.5f), new Vector2(width + thickness * 2f, thickness));
        MakeWall(wallTile, new Vector2(0f, arenaMin.y - thickness * 0.5f), new Vector2(width + thickness * 2f, thickness));
        MakeWall(wallTile, new Vector2(arenaMin.x - thickness * 0.5f, 0f), new Vector2(thickness, height));
        MakeWall(wallTile, new Vector2(arenaMax.x + thickness * 0.5f, 0f), new Vector2(thickness, height));
    }

    private void MakeWall(Sprite tile, Vector2 center, Vector2 size)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.position = new Vector3(center.x, center.y, 2f);

        SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = tile;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
        renderer.sortingOrder = -4;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static Sprite CreateWallTile()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool edge = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                float shade = edge ? 0.22f : 0.36f;
                texture.SetPixel(x, y, new Color(shade, shade * 1.02f, shade * 1.1f, 1f));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
    }

    // A clean 32x32 floor tile generated in code: light fill with darker grid lines on
    // two edges, so tiling it reads as a gridded floor. Tinted per level by the renderer.
    private static Sprite CreateFloorTile()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool gridLine = x == 0 || y == 0;
                float shade = gridLine ? 0.62f : 0.92f;
                texture.SetPixel(x, y, new Color(shade, shade, shade, 1f));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
    }

    private void BuildLevelManager(LevelData data, Transform player, GameObject melee, GameObject[] ranged)
    {
        LevelManager levelManager = new GameObject("LevelManager").AddComponent<LevelManager>();
        levelManager.arenaMin = arenaMin;
        levelManager.arenaMax = arenaMax;
        levelManager.Initialize(data, player, melee, ranged);
    }

    private void ConnectCamera(Camera camera, Transform target)
    {
        CameraFollow follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow>();
        }

        follow.target = target;
        follow.worldMin = arenaMin;
        follow.worldMax = arenaMax;
    }
}
