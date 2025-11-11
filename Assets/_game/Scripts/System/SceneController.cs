using UnityEngine;

namespace Littale {
    public class SceneController : MonoBehaviour {

        public void ChangeScene(string sceneName) {
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
