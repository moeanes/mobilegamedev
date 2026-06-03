using UnityEngine;
using UnityEngine.SceneManagement;

// A three-dot button in the top-right corner (and the Esc key) freezes the game and opens
// a menu: resume, restart, toggle music, back to the main menu, or quit. Created by
// GameSceneBootstrap, so it only exists while a level is running.
public class PauseMenu : MonoBehaviour
{
    private bool open;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private Texture2D dim;
    private Texture2D buttonBackground;
    private Texture2D dot;

    private void Update()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && gameManager.State == GameManager.GameState.Playing)
        {
            if (open)
            {
                Resume();
            }
            else
            {
                Open();
            }
        }
    }

    private void OnGUI()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            return;
        }

        EnsureStyles();

        if (open)
        {
            DrawMenu(gameManager);
            return;
        }

        // The three-dot button, only while actually playing.
        if (gameManager.State == GameManager.GameState.Playing)
        {
            Rect button = new Rect(Screen.width - 52f, 12f, 40f, 40f);
            if (GUI.Button(button, GUIContent.none))
            {
                Open();
            }

            DrawDots(button);
        }
    }

    private void DrawMenu(GameManager gameManager)
    {
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), dim);
        GUI.Label(new Rect(0f, Screen.height * 0.16f, Screen.width, 80f), "MENU", titleStyle);

        float width = Mathf.Min(300f, Screen.width * 0.62f);
        float height = 52f;
        float gap = 12f;
        float x = Screen.width * 0.5f - width * 0.5f;
        float y = Screen.height * 0.30f;

        if (GUI.Button(new Rect(x, y, width, height), "DEVAM", buttonStyle))
        {
            Resume();
        }

        if (GUI.Button(new Rect(x, y + (height + gap), width, height), "YENIDEN BASLAT", buttonStyle))
        {
            Resume();
            gameManager.RestartGame();
        }

        string musicLabel = MusicPlayer.Instance != null && MusicPlayer.Instance.IsMuted ? "MUZIK: KAPALI" : "MUZIK: ACIK";
        if (GUI.Button(new Rect(x, y + 2f * (height + gap), width, height), musicLabel, buttonStyle))
        {
            MusicPlayer.Instance?.ToggleMute();
        }

        if (GUI.Button(new Rect(x, y + 3f * (height + gap), width, height), "ANA MENU", buttonStyle))
        {
            Resume();
            gameManager.ReturnToMenu();
        }

        if (GUI.Button(new Rect(x, y + 4f * (height + gap), width, height), "CIKIS", buttonStyle))
        {
            Quit();
        }
    }

    private void DrawDots(Rect button)
    {
        const float size = 5f;
        const float spacing = 9f;
        float x = button.x + button.width * 0.5f - size * 0.5f;
        float y = button.y + button.height * 0.5f - (spacing * 2f + size) * 0.5f;
        for (int i = 0; i < 3; i++)
        {
            GUI.DrawTexture(new Rect(x, y + i * spacing, size, size), dot);
        }
    }

    private void Open()
    {
        open = true;
        GameManager.Instance?.SetPaused(true);
    }

    private void Resume()
    {
        open = false;
        GameManager.Instance?.SetPaused(false);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        dim = SolidTexture(new Color(0.03f, 0.04f, 0.07f, 0.8f));
        buttonBackground = SolidTexture(new Color(0.16f, 0.45f, 0.55f, 1f));
        dot = SolidTexture(new Color(0.9f, 0.95f, 1f, 1f));

        titleStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 50, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = new Color(0.85f, 0.95f, 1f, 1f);

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        buttonStyle.normal.background = buttonBackground;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.background = buttonBackground;
        buttonStyle.hover.textColor = new Color(0.8f, 1f, 1f, 1f);
        buttonStyle.active.background = buttonBackground;
        buttonStyle.active.textColor = Color.white;
    }

    private static Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
