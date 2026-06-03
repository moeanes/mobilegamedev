using UnityEngine;
using UnityEngine.SceneManagement;

// Background music that survives scene loads: plays the menu track in MainMenu and the
// in-game track everywhere else, switching only when the scene actually changes (so
// reloading the Game scene between levels does not restart the music). Mute is toggled
// with the M key or the on-screen button and remembered between sessions.
public class MusicPlayer : MonoBehaviour
{
    private const string MenuClipResource = "Audio/mainmenu_music";
    private const string GameClipResource = "Audio/ingame_music";
    private const string MuteKey = "music_muted";
    private const string MenuSceneName = "MainMenu";

    public static MusicPlayer Instance { get; private set; }

    private AudioSource source;
    private AudioClip menuClip;
    private AudioClip gameClip;

    public bool IsMuted => source != null && source.mute;

    // Created once by whichever scene loads first; persists from then on.
    public static void Ensure()
    {
        if (Instance == null)
        {
            new GameObject("MusicPlayer").AddComponent<MusicPlayer>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0.5f;
        source.mute = PlayerPrefs.GetInt(MuteKey, 0) == 1;

        menuClip = Resources.Load<AudioClip>(MenuClipResource);
        gameClip = Resources.Load<AudioClip>(GameClipResource);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMute();
        }
    }

    // Drawn from the persistent object, so the toggle appears in every scene (menu, game,
    // end screens) without extra wiring. Bottom-right corner, clear of the other UI.
    private void OnGUI()
    {
        string label = IsMuted ? "Muzik: Kapali (M)" : "Muzik: Acik (M)";
        if (GUI.Button(new Rect(Screen.width - 180f, Screen.height - 48f, 160f, 34f), label))
        {
            ToggleMute();
        }
    }

    public void ToggleMute()
    {
        if (source == null)
        {
            return;
        }

        source.mute = !source.mute;
        PlayerPrefs.SetInt(MuteKey, source.mute ? 1 : 0);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlayForScene(scene.name);

    private void PlayForScene(string sceneName)
    {
        AudioClip wanted = sceneName == MenuSceneName ? menuClip : gameClip;
        if (wanted == null || (source.clip == wanted && source.isPlaying))
        {
            return; // already playing the right track — don't restart on level reload
        }

        source.clip = wanted;
        source.Play();
    }
}
