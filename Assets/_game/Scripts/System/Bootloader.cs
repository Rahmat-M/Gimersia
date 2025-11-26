using UnityEngine;

namespace Littale {
    public static class Bootloader {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute() {
            GameObject systemsPrefab = Resources.Load<GameObject>("Systems");

            if (systemsPrefab != null) {
                GameObject systemsInstance = Object.Instantiate(systemsPrefab);

                Object.DontDestroyOnLoad(systemsInstance);

                systemsInstance.name = "Systems";
            } else {
#if UNITY_EDITOR
                Debug.LogWarning("Bootloader: 'Systems' prefab not found in Resources folder. Please create it to ensure Singletons are loaded.");
#endif
            }
        }
    }
}
