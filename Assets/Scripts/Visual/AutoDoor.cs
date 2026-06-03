using UnityEngine;

// An automatic door filling a doorway. It blocks like a wall until a character — player or
// enemy — comes within range, then powers down (fades out and lets them through), and
// seals again once everyone has moved away. Works for the player and the enemies, and
// fades every sprite it owns (the hazard panel plus the glowing core).
[RequireComponent(typeof(BoxCollider2D))]
public class AutoDoor : MonoBehaviour
{
    public float openRadius = 2.4f;
    public float speed = 6f;

    private static readonly int CharacterMask = (1 << GameLayers.Player) | (1 << GameLayers.Enemy);

    private BoxCollider2D doorCollider;
    private SpriteRenderer[] renderers;
    private Color[] closedColors;
    private float openAmount; // 0 = sealed, 1 = open

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        closedColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            closedColors[i] = renderers[i].color;
        }
    }

    private void Update()
    {
        bool characterNear = Physics2D.OverlapCircle(transform.position, openRadius, CharacterMask) != null;
        openAmount = Mathf.MoveTowards(openAmount, characterNear ? 1f : 0f, speed * Time.deltaTime);

        float fade = 1f - 0.88f * openAmount;
        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = closedColors[i];
            color.a = closedColors[i].a * fade;
            renderers[i].color = color;
        }

        // Passable once it is more than half open.
        doorCollider.enabled = openAmount < 0.5f;
    }
}
