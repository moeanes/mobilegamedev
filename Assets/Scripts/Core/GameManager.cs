using UnityEngine;
using UnityEngine.SceneManagement;

// Owns the overall game state and the level flow. One per Game scene (the bootstrap
// creates it). Scripts read GameManager.Instance.IsPlaying to know when to act.
public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, GameOver, GameWin }

    public static GameManager Instance { get; private set; }

    public string gameSceneName = "Game";
    public string menuSceneName = "MainMenu";

    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlaying => State == GameState.Playing;
    public bool IsGameOver => State == GameState.GameOver;

    private Hud hud;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        State = GameState.Playing;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (State == GameState.Playing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }

    public void RestartGame()
    {
        LevelDatabase.ResetToFirstLevel();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void RegisterHud(Hud newHud) => hud = newHud;

    public void ShowMessage(string message) => hud?.ShowMessage(message);

    public void OnPlayerHealthChanged(int current, int max) => hud?.SetHealth(current, max);

    public void OnLevelStarted(int level, int enemiesRemaining)
    {
        hud?.SetLevel(level);
        hud?.SetEnemiesRemaining(enemiesRemaining);
    }

    public void OnEnemiesRemainingChanged(int enemiesRemaining) => hud?.SetEnemiesRemaining(enemiesRemaining);

    public void OnPlayerDied()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        State = GameState.GameOver; // Hud draws the game-over screen from this state
    }

    public void OnLevelComplete(int currentLevel)
    {
        if (State != GameState.Playing)
        {
            return;
        }

        int nextLevel = currentLevel + 1;
        if (nextLevel > LevelDatabase.FinalLevel)
        {
            State = GameState.GameWin; // Hud draws the win screen from this state
            return;
        }

        LevelDatabase.SetPendingLevel(nextLevel);
        SceneManager.LoadScene(gameSceneName);
    }
}
