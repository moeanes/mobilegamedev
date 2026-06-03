using UnityEngine;
using UnityEngine.SceneManagement;

// Title screen: the start-screen artwork fills the background (title + doctor + hint are
// baked into it), with clickable Start / Quit button images over it. Space/Enter also start.
public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "Game";

    private Texture2D background;
    private Texture2D startButton;
    private Texture2D quitButton;

    private void Awake()
    {
        MusicPlayer.Ensure();
        background = Resources.Load<Texture2D>("UI/start_screen");
        startButton = Resources.Load<Texture2D>("UI/btn_start");
        quitButton = Resources.Load<Texture2D>("UI/btn_quit");
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
        if (background != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), background, ScaleMode.ScaleAndCrop);
        }

        float centerX = Screen.width * 0.5f;
        float width = Mathf.Min(320f, Screen.width * 0.5f);
        float y = Screen.height * 0.62f;
        float gap = 14f;

        if (UiButton.Draw(startButton, centerX, y, width))
        {
            StartGame();
        }

        if (UiButton.Draw(quitButton, centerX, y + UiButton.Height(quitButton, width) + gap, width))
        {
            Quit();
        }
    }
}
