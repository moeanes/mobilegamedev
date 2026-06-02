using UnityEngine;

// Top-down WASD / arrow-key movement, clamped to the arena. Legacy Input Manager.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector2 arenaMin = new Vector2(-13f, -8f);
    public Vector2 arenaMax = new Vector2(13f, 8f);

    private Rigidbody2D body;
    private Vector2 moveInput;

    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 next = body.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        next.x = Mathf.Clamp(next.x, arenaMin.x, arenaMax.x);
        next.y = Mathf.Clamp(next.y, arenaMin.y, arenaMax.y);
        body.MovePosition(next);
    }
}
