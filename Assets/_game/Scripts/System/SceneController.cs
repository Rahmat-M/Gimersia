using UnityEngine;

namespace Littale {
    public class SceneController : MonoBehaviour {

        public void ChangeScene(string sceneName) {
            Time.timeScale = 1.0f; // Reset time scale in case it was modified
            SceneLoader.LoadScene(sceneName);
        }

        public void CloseGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}
