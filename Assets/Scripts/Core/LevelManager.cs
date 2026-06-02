using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns enemies around the player over time and ends the level once enough are killed.
// Survival win condition: kill the level's target count of enemies.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public Vector2 arenaMin = new Vector2(-13f, -8f);
    public Vector2 arenaMax = new Vector2(13f, 8f);
    public float spawnRingMin = 7f;
    public float spawnRingMax = 11f;
    public float levelCompleteDelay = 1.5f;

    private LevelData data;
    private Transform player;
    private GameObject meleeTemplate;
    private GameObject[] rangedTemplates;
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

    public void Initialize(LevelData levelData, Transform playerTransform, GameObject melee, GameObject[] ranged)
    {
        data = levelData;
        player = playerTransform;
        meleeTemplate = melee;
        rangedTemplates = ranged;
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

        if (!levelComplete && killedCount >= data.enemiesToKill)
        {
            levelComplete = true;
            StartCoroutine(LevelCompleteRoutine());
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (spawnedCount < data.enemiesToKill)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield break;
            }

            aliveEnemies.RemoveAll(enemy => enemy == null);
            if (aliveEnemies.Count < data.maxConcurrentAlive)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(data.spawnInterval);
        }
    }

    private IEnumerator LevelCompleteRoutine()
    {
        GameManager.Instance?.ShowMessage("Bolum tamamlandi!");
        yield return new WaitForSeconds(levelCompleteDelay);
        GameManager.Instance?.OnLevelComplete(data.levelIndex);
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

    private Vector2 SpawnPosition()
    {
        Vector2 center = player != null ? (Vector2)player.position : Vector2.zero;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnRingMin, spawnRingMax);

        Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        candidate.x = Mathf.Clamp(candidate.x, arenaMin.x, arenaMax.x);
        candidate.y = Mathf.Clamp(candidate.y, arenaMin.y, arenaMax.y);
        return candidate;
    }
}
