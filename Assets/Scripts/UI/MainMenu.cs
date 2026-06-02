using UnityEngine;
using UnityEngine.SceneManagement;

// Animated title screen drawn with IMGUI: pulsing title, an idle-animating doctor,
// and clickable Start / Quit buttons (Space/Enter also start).
public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "Game";

    private const int DoctorCols = 2;
    private const int DoctorRows = 3;
    private const int DoctorFrames = 6;

    private Texture2D doctorSheet;
    private Texture2D buttonNormal;
    private Texture2D buttonHover;
    private GUIStyle titleStyle;
    private GUIStyle hintStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        doctorSheet = Resources.Load<Texture2D>("Characters/doctor_idle");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        LevelDatabase.ResetToFirstLevel();
        SceneManager.LoadScene(gameSceneName);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGUI()
    {
        EnsureStyles();

        float centerX = Screen.width * 0.5f;
        float time = Time.unscaledTime;

        // Pulsing title.
        float pulse = 1f + Mathf.Sin(time * 2.2f) * 0.045f;
        titleStyle.fontSize = Mathf.RoundToInt(46f * pulse);
        GUI.Label(new Rect(0f, Screen.height * 0.12f, Screen.width, 110f), "Doctor : Last Doctor Standing", titleStyle);

        // Idle-animating, gently bobbing doctor.
        if (doctorSheet != null)
        {
            int frame = (int)(time * 6f) % DoctorFrames;
            int col = frame % DoctorCols;
            int row = frame / DoctorCols;
            var texCoords = new Rect(col / (float)DoctorCols, 1f - (row + 1f) / DoctorRows, 1f / DoctorCols, 1f / DoctorRows);

            float bob = Mathf.Sin(time * 2f) * 7f;
            const float doctorWidth = 150f;
            const float doctorHeight = 225f;
            GUI.DrawTextureWithTexCoords(
                new Rect(centerX - doctorWidth * 0.5f, Screen.height * 0.30f + bob, doctorWidth, doctorHeight),
                doctorSheet,
                texCoords);
        }

        // Buttons.
        const float buttonWidth = 280f;
        const float buttonHeight = 64f;
        const float gap = 18f;
        float buttonY = Screen.height * 0.66f;

        if (GUI.Button(new Rect(centerX - buttonWidth * 0.5f, buttonY, buttonWidth, buttonHeight), "BASLA", buttonStyle))
        {
            StartGame();
        }

        if (GUI.Button(new Rect(centerX - buttonWidth * 0.5f, buttonY + buttonHeight + gap, buttonWidth, buttonHeight), "CIKIS", buttonStyle))
        {
            Quit();
        }

        GUI.Label(
            new Rect(0f, Screen.height * 0.9f, Screen.width, 50f),
            "WASD hareket   ·   Sol tik ates   ·   5 bolumu bitir",
            hintStyle);
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = new Color(0.85f, 0.95f, 1f, 1f);

        hintStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 22 };
        hintStyle.normal.textColor = new Color(0.70f, 0.76f, 0.84f, 1f);

        buttonNormal = SolidTexture(new Color(0.16f, 0.45f, 0.55f, 0.95f));
        buttonHover = SolidTexture(new Color(0.24f, 0.64f, 0.74f, 1f));

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        buttonStyle.normal.background = buttonNormal;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.background = buttonHover;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.background = buttonHover;
        buttonStyle.active.textColor = Color.white;
        buttonStyle.focused.background = buttonNormal;
        buttonStyle.focused.textColor = Color.white;
    }

    private static Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
