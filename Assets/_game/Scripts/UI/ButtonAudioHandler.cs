using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Littale {
    [RequireComponent(typeof(Button))]
    public class ButtonAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler {

        [Header("Audio Name (from AudioManager)")]
        public string hoverSoundName = "cursor";
        public string pressSoundName = "select";

        private Button button;

        void Awake() {
            button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (button != null && button.interactable && !string.IsNullOrEmpty(hoverSoundName)) {
                SoundManager.Instance.Play(hoverSoundName);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (button != null && button.interactable && !string.IsNullOrEmpty(pressSoundName)) {
                SoundManager.Instance.Play(pressSoundName);
            }
        }

    }
}
