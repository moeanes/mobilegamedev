using System.Collections;
using UnityEngine;

// The end-of-game boss: a giant virus that drops into the arena (smashing the interior
// walls into one open room as it lands), chases the doctor for contact damage, and fires
// telegraphed projectile volleys on a timer. It escalates through three phases as its
// health drops, swapping to an angrier face and attacking harder. Reuses EnemyHealth for
// hit points so the doctor's bullets damage it like any other enemy.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class BossEnemy : MonoBehaviour
{
    public float moveSpeed = 1.8f;
    public int contactDamage = 1;
    public float contactCooldown = 0.9f;

    public GameObject projectilePrefab;
    public float projectileSpeed = 6.5f;
    public int projectileDamage = 1;

    private const string FaceSheet = "Characters/boss";

    // Which sheet frame to show per phase (1->index 0). The 3x3 sheet reads angrier left to
    // right, top to bottom; tweak these if a different face fits a phase better.
    private static readonly int[] PhaseFace = { 0, 3, 6 };

    // Per-phase attack tuning (phase 1 = full health, 3 = desperate). Kept gentle for the
    // "medium" difficulty we agreed on; numbers are tuned by playtest.
    private static readonly float[] PhaseFireInterval = { 3.2f, 2.4f, 1.7f };
    private static readonly int[] PhaseShots = { 1, 3, 5 };
    private static readonly float[] PhaseSpeedScale = { 1f, 1.15f, 1.35f };

    private Rigidbody2D body;
    private EnemyHealth health;
    private CircleCollider2D circle;
    private SpriteRenderer face;
    private Sprite[] faces;
    private Transform target;
    private RoomMap map;

    private float radius = 1.6f;
    private float lastContactTime = -999f;
    private float fireTimer;
    private int phase = 1;
    private bool fighting;

    public void Init(Transform player, RoomMap roomMap)
    {
        target = player;
        map = roomMap;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;

        health = GetComponent<EnemyHealth>();
        circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            radius = circle.radius;
        }

        face = GetComponentInChildren<SpriteRenderer>();
        faces = SpriteSheetLoader.Load(FaceSheet, 64, 64, 7);
    }

    private void OnEnable()
    {
        health.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        health.HealthChanged -= OnHealthChanged;
    }

    private void Start()
    {
        SetFace(1);
        GameManager.Instance?.OnBossHealthChanged(health.CurrentHealth, health.maxHealth);
        StartCoroutine(DropIn());
    }

    // Falls from above the arena to its centre, then smashes the interior walls open and
    // starts fighting. The collider is off during the fall, so it can't be shot or shoved
    // until it lands.
    private IEnumerator DropIn()
    {
        if (circle != null)
        {
            circle.enabled = false;
        }

        Vector2 landing = map != null ? (map.WorldMin + map.WorldMax) * 0.5f : (Vector2)transform.position;
        Vector2 startAbove = landing + Vector2.up * 9f;
        transform.position = startAbove;

        GameManager.Instance?.ShowMessage("BOSS!");

        float elapsed = 0f;
        const float fallTime = 1.1f;
        while (elapsed < fallTime)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / fallTime);
            transform.position = Vector2.Lerp(startAbove, landing, k);
            yield return null;
        }

        transform.position = landing;
        MapBuilder.OpenArena(map);
        GameManager.Instance?.ShowMessage(string.Empty);

        if (circle != null)
        {
            circle.enabled = true;
        }

        fighting = true;
    }

    private void FixedUpdate()
    {
        if (!fighting || target == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
        {
            return;
        }

        Vector2 toTarget = (Vector2)target.position - body.position;
        Vector2 direction = EnemyNavigation.Steer(body.position, toTarget, radius);
        float speed = moveSpeed * PhaseSpeedScale[phase - 1];
        body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (!fighting || target == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
        {
            return;
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = PhaseFireInterval[phase - 1];
            StartCoroutine(TelegraphAndFire());
        }
    }

    // A short wind-up (the face flashes red) before the volley, so the player has time to
    // read it and dodge — the core of a fair boss attack.
    private IEnumerator TelegraphAndFire()
    {
        if (face != null)
        {
            face.color = new Color(1f, 0.55f, 0.55f, 1f);
        }

        yield return new WaitForSeconds(0.45f);

        if (face != null)
        {
            face.color = Color.white;
        }

        if (fighting && target != null && projectilePrefab != null)
        {
            FireVolley(PhaseShots[phase - 1]);
        }
    }

    private void FireVolley(int shots)
    {
        Vector2 aim = ((Vector2)target.position - body.position).normalized;
        if (aim.sqrMagnitude < 0.0001f)
        {
            aim = Vector2.down;
        }

        const float spread = 24f; // degrees between neighbouring shots
        float start = -spread * (shots - 1) * 0.5f;
        for (int i = 0; i < shots; i++)
        {
            Vector2 direction = Rotate(aim, start + spread * i);
            GameObject shot = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            shot.SetActive(true);

            EnemyProjectile projectile = shot.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                projectile.speed = projectileSpeed;
                projectile.damage = projectileDamage;
                projectile.Launch(direction);
            }
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        GameManager.Instance?.OnBossHealthChanged(current, max);

        int next = current <= max / 3 ? 3 : (current <= 2 * max / 3 ? 2 : 1);
        if (next != phase)
        {
            phase = next;
            SetFace(phase);
        }
    }

    private void SetFace(int phaseNumber)
    {
        if (face == null || faces == null || faces.Length == 0)
        {
            return;
        }

        int frame = Mathf.Clamp(PhaseFace[Mathf.Clamp(phaseNumber - 1, 0, PhaseFace.Length - 1)], 0, faces.Length - 1);
        face.sprite = faces[frame];
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!fighting || Time.time < lastContactTime + contactCooldown)
        {
            return;
        }

        PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
            lastContactTime = Time.time;
        }
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
