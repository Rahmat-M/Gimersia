using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Littale {
    [RequireComponent(typeof(Button))]
    public class ButtonAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler {

        [Header("Audio Settings")]
        public SfxID hoverSound;
        public SfxID clickSound;

        private Button button;

        void Awake() {
            button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (button != null && button.interactable && hoverSound != SfxID.None) {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(hoverSound);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (button != null && button.interactable && clickSound != SfxID.None) {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(clickSound);
            }
        }

    }
}
