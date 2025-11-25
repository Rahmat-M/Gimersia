using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader {
    public static string TargetSceneName { get; private set; }

    private const string LOADING_SCENE_NAME = "LoadingScene";

    public static void LoadScene(string targetScene) {
        TargetSceneName = targetScene;

        SceneManager.LoadScene(LOADING_SCENE_NAME);
    }
}