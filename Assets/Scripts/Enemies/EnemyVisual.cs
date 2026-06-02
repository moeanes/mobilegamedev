using UnityEngine;

// Cycles the enemy's sprite sheet and flips it toward its movement direction.
// Lives on the enemy's "Visual" child.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SimpleSpriteAnimator))]
public class EnemyVisual : MonoBehaviour
{
    public string sheetResource = "Characters/enemy1";
    public int frameWidth = 32;
    public int frameHeight = 32;
    public int frameCount = 0; // 0 = use every cell; set when the sheet has blank cells
    public float framesPerSecond = 8f;

    private SpriteRenderer spriteRenderer;
    private SimpleSpriteAnimator animator;
    private Transform root;
    private float lastX;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<SimpleSpriteAnimator>();
        root = transform.parent != null ? transform.parent : transform;
    }

    private void Start()
    {
        lastX = root.position.x;
        Sprite[] frames = SpriteSheetLoader.Load(sheetResource, frameWidth, frameHeight, frameCount);
        animator.Play(frames, framesPerSecond, true);
    }

    private void Update()
    {
        float dx = root.position.x - lastX;
        if (Mathf.Abs(dx) > 0.001f)
        {
            spriteRenderer.flipX = dx < 0f;
        }

        lastX = root.position.x;
    }
}
