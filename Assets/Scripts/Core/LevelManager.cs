using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns enemies around the player over time and ends the level once enough are killed.
// Survival win condition: kill the level's target count of enemies.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // How far from the player enemies appear. Sized for a single room so they spawn on
    // reachable floor nearby rather than across a wall.
    public float spawnRingMin = 4f;
    public float spawnRingMax = 8f;
    public float levelCompleteDelay = 1.5f;

    // An enemy farther than this from the player is treated as stranded (stuck behind a
    // wall) and relocated. Larger than the spawn ring and off-screen, so it never yanks
    // an enemy the player can see.
    private const float StrandedDistance = 16f;

    private static readonly int WallMask = 1 << GameLayers.Wall;

    private RoomMap map;
    private LevelData data;
    private Transform player;
    private GameObject meleeTemplate;
    private GameObject[] rangedTemplates;
    private GameObject bossTemplate;
    private GameObject bossProjectile;
    private bool bossSpawned;
    private readonly List<EnemyHealth> aliveEnemies = new List<EnemyHealth>();
    private int spawnedCount;
    private int killedCount;
    private bool levelComplete;

    public int RemainingToKill => data != null ? Mathf.Max(0, data.enemiesToKill - killedCount) : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Initialize(RoomMap roomMap, LevelData levelData, Transform playerTransform, GameObject melee, GameObject[] ranged, GameObject boss, GameObject bossProjectileTemplate)
    {
        map = roomMap;
        data = levelData;
        player = playerTransform;
        meleeTemplate = melee;
        rangedTemplates = ranged;
        bossTemplate = boss;
        bossProjectile = bossProjectileTemplate;
    }

    private void Start()
    {
        GameManager.Instance?.OnLevelStarted(data.levelIndex, RemainingToKill);
        StartCoroutine(SpawnLoop());
    }

    public void NotifyEnemyKilled(EnemyHealth enemy)
    {
        aliveEnemies.Remove(enemy);
        killedCount++;
        GameManager.Instance?.OnEnemiesRemainingChanged(RemainingToKill);

        if (levelComplete || killedCount < data.enemiesToKill)
        {
            return;
        }

        if (data.hasBoss)
        {
            if (!bossSpawned)
            {
                SpawnBoss();
            }

            return; // once the boss is in, only its death finishes the level
        }

        levelComplete = true;
        StartCoroutine(LevelCompleteRoutine());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.5f);

        // Runs until the level is finished (not just until everything is spawned) so the
        // stranded-enemy rescue keeps working after the last spawn.
        while (!levelComplete)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield break;
            }

            aliveEnemies.RemoveAll(enemy => enemy == null);

            if (spawnedCount < data.enemiesToKill && aliveEnemies.Count < data.maxConcurrentAlive)
            {
                SpawnEnemy();
            }

            RescueStrandedEnemies();

            yield return new WaitForSeconds(data.spawnInterval);
        }
    }

    // Enemies have no real pathfinding, so one could get left behind a wall when the
    // player changes rooms and never be reachable again — which would stall the level
    // (the kill target could never be met). Any enemy that ends up far across the map is
    // teleported back to a fresh reachable spot near the player. They are off-screen at
    // that distance, so the move is never seen.
    private void RescueStrandedEnemies()
    {
        if (player == null)
        {
            return;
        }

        Vector2 playerPosition = player.position;
        foreach (EnemyHealth enemy in aliveEnemies)
        {
            if (enemy != null && Vector2.Distance(enemy.transform.position, playerPosition) > StrandedDistance)
            {
                enemy.transform.position = SpawnPosition();
            }
        }
    }

    private IEnumerator LevelCompleteRoutine()
    {
        GameManager.Instance?.ShowMessage("Bolum tamamlandi!");
        yield return new WaitForSeconds(levelCompleteDelay);
        GameManager.Instance?.OnLevelComplete(data.levelIndex);
    }

    // Final level: instead of finishing when the kill target is hit, drop the boss in. The
    // level is won only when the boss dies (its EnemyHealth.Died -> OnBossDefeated).
    private void SpawnBoss()
    {
        if (bossTemplate == null)
        {
            levelComplete = true;
            StartCoroutine(LevelCompleteRoutine());
            return;
        }

        bossSpawned = true;

        Vector2 center = (map.WorldMin + map.WorldMax) * 0.5f;
        GameObject boss = Instantiate(bossTemplate, center, Quaternion.identity);
        boss.SetActive(true);

        BossEnemy bossEnemy = boss.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            bossEnemy.projectilePrefab = bossProjectile;
            bossEnemy.Init(player, map);
        }

        EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();
        if (bossHealth != null)
        {
            bossHealth.reportToLevelManager = false; // its death wins the level, not a kill
            bossHealth.Died += OnBossDefeated;
        }
    }

    private void OnBossDefeated()
    {
        if (levelComplete)
        {
            return;
        }

        levelComplete = true;
        GameManager.Instance?.HideBossBar();
        StartCoroutine(LevelCompleteRoutine());
    }

    private void SpawnEnemy()
    {
        bool canSpawnRanged = rangedTemplates != null && rangedTemplates.Length > 0
            && data.levelIndex >= data.rangedFromLevel
            && data.rangedWeight > 0f;
        bool spawnRanged = canSpawnRanged && Random.value < data.rangedWeight;

        GameObject template = spawnRanged
            ? rangedTemplates[Random.Range(0, rangedTemplates.Length)]
            : meleeTemplate;
        if (template == null)
        {
            return;
        }

        GameObject enemy = Instantiate(template, SpawnPosition(), Quaternion.identity);
        enemy.SetActive(true);

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            aliveEnemies.Add(health); // fixed health per type; difficulty comes from counts/mix
        }

        MeleeEnemy melee = enemy.GetComponent<MeleeEnemy>();
        if (melee != null)
        {
            melee.SetTarget(player);
        }

        RangedEnemy ranged = enemy.GetComponent<RangedEnemy>();
        if (ranged != null)
        {
            ranged.SetTarget(player);
        }

        spawnedCount++;
    }

    // Picks a floor cell in the spawn ring that the new enemy can walk to in a straight
    // line (no wall between it and the player). Falls back to any floor in the ring, then
    // any floor outside the inner radius, so a spawn is always found. One pass over the
    // floor with reservoir sampling keeps each eligible spot equally likely.
    private Vector2 SpawnPosition()
    {
        Vector2 center = player != null ? (Vector2)player.position : Vector2.zero;
        if (map == null)
        {
            return center;
        }

        Vector2 lineOfSight = center;
        Vector2 inRing = center;
        Vector2 anyFar = center;
        int seenLineOfSight = 0;
        int seenInRing = 0;
        int seenAnyFar = 0;

        foreach (Vector2 point in map.FloorPoints)
        {
            float distance = Vector2.Distance(point, center);
            if (distance < spawnRingMin)
            {
                continue;
            }

            if (Random.Range(0, ++seenAnyFar) == 0)
            {
                anyFar = point;
            }

            if (distance > spawnRingMax)
            {
                continue;
            }

            if (Random.Range(0, ++seenInRing) == 0)
            {
                inRing = point;
            }

            if (!Physics2D.Linecast(center, point, WallMask) && Random.Range(0, ++seenLineOfSight) == 0)
            {
                lineOfSight = point;
            }
        }

        if (seenLineOfSight > 0)
        {
            return lineOfSight;
        }

        return seenInRing > 0 ? inRing : anyFar;
    }
}
