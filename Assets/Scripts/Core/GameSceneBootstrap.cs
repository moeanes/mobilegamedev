using UnityEngine;

// Sits in the (otherwise empty) Game scene and builds everything in code when it loads:
// camera, room-based map, GameManager, player, enemy templates, level manager, HUD.
// Which level it builds comes from LevelDatabase.PendingLevel.
[DefaultExecutionOrder(-100)]
public class GameSceneBootstrap : MonoBehaviour
{
    public float cameraSize = 6.5f;

    private void Awake()
    {
        LevelData data = LevelDatabase.Get(LevelDatabase.PendingLevel);
        RoomMap map = new RoomMap(data.levelIndex);

        MusicPlayer.Ensure();
        ConfigureCollisions();
        EnsureGameManager();
        Camera camera = EnsureCamera();

        MapBuilder.Build(map);
        MapDecorator.Decorate(map);

        GameObject bulletTemplate = EnemyFactory.CreatePlayerBulletTemplate();

        // Keep the player a little inside the outer wall so it never overlaps it.
        Vector2 playerMin = map.WorldMin + Vector2.one;
        Vector2 playerMax = map.WorldMax - Vector2.one;
        GameObject player = PlayerFactory.Create(map.PlayerSpawn, playerMin, playerMax, bulletTemplate);

        GameObject blueProjectile = EnemyFactory.CreateEnemyProjectileTemplate("Projectiles/enemy3_projectile");
        GameObject pinkProjectile = EnemyFactory.CreateEnemyProjectileTemplate("Projectiles/enemy_projectile");

        GameObject meleeTemplate = EnemyFactory.CreateMeleeTemplate();
        GameObject blueRanged = EnemyFactory.CreateRangedTemplate("BlueRanged", "Characters/enemy3", 5, 2, 2f, blueProjectile);
        GameObject pinkRanged = EnemyFactory.CreateRangedTemplate("PinkRanged", "Characters/enemy2", 1, 2, 1f, pinkProjectile);

        BuildLevelManager(map, data, player.transform, meleeTemplate, new[] { blueRanged, pinkRanged });
        Hud.Create();
        new GameObject("PauseMenu").AddComponent<PauseMenu>();
        FitCamera(camera, map);
    }

    private static void ConfigureCollisions()
    {
        // Enemies don't push each other (no clumping). They DO collide with furniture, but
        // EnemyNavigation steers them around it so they never get stuck on it.
        Physics2D.IgnoreLayerCollision(GameLayers.Enemy, GameLayers.Enemy, true);
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

    private void BuildLevelManager(RoomMap map, LevelData data, Transform player, GameObject melee, GameObject[] ranged)
    {
        LevelManager levelManager = new GameObject("LevelManager").AddComponent<LevelManager>();
        levelManager.Initialize(map, data, player, melee, ranged);
    }

    private void FitCamera(Camera camera, RoomMap map)
    {
        CameraFitMap fit = camera.GetComponent<CameraFitMap>();
        if (fit == null)
        {
            fit = camera.gameObject.AddComponent<CameraFitMap>();
        }

        fit.worldMin = map.WorldMin;
        fit.worldMax = map.WorldMax;
    }
}
