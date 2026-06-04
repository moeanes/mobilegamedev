using UnityEngine;

// In-game UI (IMGUI). During play: hearts (top-left) cut from Resources/UI/HealthUI.png,
// level + remaining enemies as text, and a panelled centre message for transient notices.
// On game over / win it draws a full-screen end screen with Restart / Menu buttons.
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
    private int bossHealth;
    private int bossMax;
    private bool bossVisible;

    private GUIStyle infoStyle;
    private GUIStyle centerStyle;
    private Texture2D heartSheet;
    private Texture2D panel;
    private Texture2D gameOverImage;
    private Texture2D restartButton;
    private Texture2D menuButton;
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

    public void SetBossHealth(int current, int max)
    {
        bossHealth = current;
        bossMax = max;
        bossVisible = max > 0;
    }

    public void HideBoss() => bossVisible = false;

    private void OnGUI()
    {
        EnsureResources();

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.State == GameManager.GameState.GameOver)
        {
            DrawEndScreen(gameOverImage, null);
            return;
        }

        if (gameManager != null && gameManager.State == GameManager.GameState.GameWin)
        {
            DrawEndScreen(null, "KAZANDIN!");
            return;
        }

        DrawHearts();

        infoStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(Screen.width * 0.5f - 130f, 16f, 260f, 38f), "Bolum " + level, infoStyle);

        infoStyle.alignment = TextAnchor.MiddleRight;
        // Shifted left to leave the top-right corner for the menu (three-dot) button.
        GUI.Label(new Rect(Screen.width - 415f, 16f, 340f, 38f), "Kalan dusman: " + enemiesRemaining, infoStyle);

        if (!string.IsNullOrEmpty(centerMessage))
        {
            const float panelWidth = 760f;
            const float panelHeight = 220f;
            var panelRect = new Rect(Screen.width * 0.5f - panelWidth * 0.5f, Screen.height * 0.5f - panelHeight * 0.5f, panelWidth, panelHeight);
            GUI.DrawTexture(panelRect, panel);
            GUI.Label(panelRect, centerMessage, centerStyle);
        }

        if (bossVisible && bossMax > 0)
        {
            DrawBossBar();
        }
    }

    // Full-screen end screen: artwork background (game over) or a dimmed headline (win),
    // with Restart and Menu buttons under it.
    private void DrawEndScreen(Texture2D artwork, string headline)
    {
        if (artwork != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), artwork, ScaleMode.ScaleAndCrop);
        }
        else
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), panel);
            if (!string.IsNullOrEmpty(headline))
            {
                GUI.Label(new Rect(0f, Screen.height * 0.28f, Screen.width, 120f), headline, centerStyle);
            }
        }

        GameManager gameManager = GameManager.Instance;

        // Restart on the far left, menu on the far right, so the fallen doctor stays
        // visible in the open centre between them.
        float buttonWidth = Mathf.Min(280f, Screen.width * 0.25f);
        float buttonTop = Screen.height * 0.80f - UiButton.Height(restartButton, buttonWidth) * 0.5f;
        float leftX = Screen.width * 0.19f;
        float rightX = Screen.width * 0.81f;

        if (UiButton.Draw(restartButton, leftX, buttonTop, buttonWidth))
        {
            gameManager?.RestartGame();
        }

        if (UiButton.Draw(menuButton, rightX, buttonTop, buttonWidth))
        {
            gameManager?.ReturnToMenu();
        }
    }

    // Wide boss health bar across the bottom, with a "BOSS" label. Green, turning red in the
    // final third so the player feels the boss closing in.
    private void DrawBossBar()
    {
        float barWidth = Mathf.Min(620f, Screen.width * 0.5f);
        const float barHeight = 22f;
        float x = Screen.width * 0.5f - barWidth * 0.5f;
        float y = Screen.height - 56f;

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(x - 4f, y - 4f, barWidth + 8f, barHeight + 8f), Texture2D.whiteTexture);

        GUI.color = new Color(0.14f, 0.14f, 0.18f, 1f);
        GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), Texture2D.whiteTexture);

        float fraction = Mathf.Clamp01((float)bossHealth / bossMax);
        GUI.color = fraction < 0.34f ? new Color(0.86f, 0.26f, 0.28f) : new Color(0.49f, 0.84f, 0.32f);
        GUI.DrawTexture(new Rect(x, y, barWidth * fraction, barHeight), Texture2D.whiteTexture);

        GUI.color = Color.white;
        infoStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y - 30f, barWidth, 28f), "BOSS", infoStyle);
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
            gameOverImage = Resources.Load<Texture2D>("UI/game_over");
            restartButton = Resources.Load<Texture2D>("UI/btn_restart");
            menuButton = Resources.Load<Texture2D>("UI/btn_menu");

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
