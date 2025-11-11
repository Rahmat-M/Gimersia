using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Littale {
    public class LoadingController : MonoBehaviour {
        public string fallbackScene = "MenuScene";

        public float minimumWaitTime = 3.0f;

        void Start() {
            StartCoroutine(LoadSceneAsync());
        }

        IEnumerator LoadSceneAsync() {
            float startTime = Time.time;

            string sceneToLoad;
            if (!string.IsNullOrEmpty(SceneLoader.TargetSceneName)) {
                sceneToLoad = SceneLoader.TargetSceneName;
            } else {
                Debug.LogWarning("Cannot find target scene, going to fallback scene : " + fallbackScene);
                sceneToLoad = fallbackScene;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            asyncLoad.allowSceneActivation = false;

            float timeElapsed = Time.time - startTime;
            if (timeElapsed < minimumWaitTime) {
                yield return new WaitForSeconds(minimumWaitTime - timeElapsed);
            }

            asyncLoad.allowSceneActivation = true;
        }
    }
}