using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time project wiring: creates the MainMenu and Game scenes (each nearly empty —
// just a camera and the controller component) and registers them in Build Settings.
// Run from the Unity menu "Game > Setup Scenes" or in batch mode via -executeMethod.
public static class GameSetupEditor
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string MenuScenePath = ScenesFolder + "/MainMenu.unity";
    private const string GameScenePath = ScenesFolder + "/Game.unity";

    [MenuItem("Game/Setup Scenes")]
    public static void SetupScenes()
    {
        if (!AssetDatabase.IsValidFolder(ScenesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        CreateMenuScene();
        CreateGameScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[GameSetupEditor] MainMenu + Game scenes created and registered in Build Settings.");
    }

    private static void CreateMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        GameObject menu = new GameObject("MainMenu");
        menu.AddComponent<MainMenu>();

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject bootstrap = new GameObject("GameSceneBootstrap");
        bootstrap.AddComponent<GameSceneBootstrap>();

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }
}
