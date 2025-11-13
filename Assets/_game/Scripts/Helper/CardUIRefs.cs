using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class CardUIRefs {
        public GameObject root;
        public Image mainImage;
        public Image cooldownOverlay;
        public TextMeshProUGUI cooldownText;

        public CardUIRefs(GameObject obj) {
            root = obj;
            mainImage = obj.GetComponent<Image>();

            Transform overlayT = root.transform.Find("CooldownOverlay");
            if (overlayT != null) {
                cooldownOverlay = overlayT.GetComponent<Image>();
            }

            Transform textT = root.transform.Find("CooldownText");
            if (textT != null) {
                cooldownText = textT.GetComponent<TextMeshProUGUI>();
            }

            // Peringatan jika setup prefab salah
            if (cooldownOverlay == null)
                Debug.LogWarning($"[CardUIRefs] Tidak menemukan child 'CooldownOverlay' di prefab {obj.name}");
            if (cooldownText == null)
                Debug.LogWarning($"[CardUIRefs] Tidak menemukan child 'CooldownText' di prefab {obj.name}");
        }
    }
}