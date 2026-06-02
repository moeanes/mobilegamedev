using UnityEngine;

// Picks the doctor animation (idle / walk / hit) and flips the sprite toward the mouse.
// Lives on the "Visual" child; reads movement + health from the parent.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SimpleSpriteAnimator))]
public class PlayerVisual : MonoBehaviour
{
    public float hitClipDuration = 0.35f;

    private SpriteRenderer spriteRenderer;
    private SimpleSpriteAnimator animator;
    private PlayerMovement movement;
    private PlayerHealth health;
    private Camera mainCamera;

    private Sprite[] idleFrames;
    private Sprite[] walkFrames;
    private Sprite[] hitFrames;
    private float hitTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<SimpleSpriteAnimator>();
        movement = GetComponentInParent<PlayerMovement>();
        health = GetComponentInParent<PlayerHealth>();
        mainCamera = Camera.main;

        idleFrames = SpriteSheetLoader.Load("Characters/doctor_idle", 32, 32);
        walkFrames = SpriteSheetLoader.Load("Characters/doctor_walk", 32, 32);
        hitFrames = SpriteSheetLoader.Load("Characters/doctor_hit", 32, 32);
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += OnDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= OnDamaged;
        }
    }

    private void OnDamaged()
    {
        hitTimer = hitClipDuration;
    }

    private void Update()
    {
        FlipTowardMouse();

        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;
            animator.Play(hitFrames, 12f, true);
            return;
        }

        bool walking = movement != null && movement.MoveInput.sqrMagnitude > 0.01f;
        animator.Play(walking ? walkFrames : idleFrames, 10f, true);
    }

    private void FlipTowardMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        spriteRenderer.flipX = mouseWorld.x < transform.position.x;
    }
}
