using UnityEngine;

// In-game UI (IMGUI). Health shows as hearts (top-left) cut from Resources/UI/HealthUI.png;
// level + remaining enemies as text; a panelled center message for win/lose/cleared.
public class Hud : MonoBehaviour
{
    // HealthUI.png is a 3x7 grid of 11px hearts. The top-left cell is a full red heart.
    private const int SheetCols = 3;
    private const int SheetRows = 7;
    private const int HeartCol = 0;
    private const int HeartRow = 0;

    private int health;
    private int maxHealth = 5;
    private int level = 1;
    private int enemiesRemaining;
    private string centerMessage;

    private GUIStyle infoStyle;
    private GUIStyle centerStyle;
    private Texture2D heartSheet;
    private Texture2D panel;
    private Rect heartUV;

    public static Hud Create()
    {
        GameObject hudObject = new GameObject("HUD");
        return hudObject.AddComponent<Hud>();
    }

    private void Awake()
    {
        GameManager.Instance?.RegisterHud(this);
    }

    public void SetHealth(int current, int max)
    {
        health = current;
        maxHealth = max;
    }

    public void SetLevel(int value) => level = value;

    public void SetEnemiesRemaining(int value) => enemiesRemaining = value;

    public void ShowMessage(string message) => centerMessage = message;

    private void OnGUI()
    {
        EnsureResources();

        DrawHearts();

        infoStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(Screen.width * 0.5f - 130f, 16f, 260f, 38f), "Bolum " + level, infoStyle);

        infoStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(Screen.width - 360f, 16f, 340f, 38f), "Kalan dusman: " + enemiesRemaining, infoStyle);

        if (!string.IsNullOrEmpty(centerMessage))
        {
            const float panelWidth = 760f;
            const float panelHeight = 220f;
            var panelRect = new Rect(Screen.width * 0.5f - panelWidth * 0.5f, Screen.height * 0.5f - panelHeight * 0.5f, panelWidth, panelHeight);
            GUI.DrawTexture(panelRect, panel);
            GUI.Label(panelRect, centerMessage, centerStyle);
        }
    }

    private void DrawHearts()
    {
        const float size = 34f;
        const float gap = 6f;

        for (int i = 0; i < maxHealth; i++)
        {
            var rect = new Rect(18f + i * (size + gap), 16f, size, size);

            if (heartSheet != null)
            {
                GUI.color = i < health ? Color.white : new Color(1f, 1f, 1f, 0.22f);
                GUI.DrawTextureWithTexCoords(rect, heartSheet, heartUV);
            }
            else
            {
                GUI.color = i < health ? new Color(0.92f, 0.24f, 0.30f) : new Color(0.3f, 0.3f, 0.34f, 0.6f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
            }
        }

        GUI.color = Color.white;
    }

    private void EnsureResources()
    {
        if (infoStyle == null)
        {
            infoStyle = new GUIStyle { fontSize = 22, fontStyle = FontStyle.Bold };
            infoStyle.normal.textColor = Color.white;

            centerStyle = new GUIStyle { fontSize = 46, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            centerStyle.normal.textColor = new Color(1f, 0.9f, 0.32f, 1f);
        }

        if (panel == null)
        {
            panel = SolidTexture(new Color(0.05f, 0.06f, 0.09f, 0.82f));
            heartSheet = Resources.Load<Texture2D>("UI/HealthUI");

            float cellWidth = 1f / SheetCols;
            float cellHeight = 1f / SheetRows;
            heartUV = new Rect(HeartCol * cellWidth, 1f - (HeartRow + 1) * cellHeight, cellWidth, cellHeight);
        }
    }

    private static Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
