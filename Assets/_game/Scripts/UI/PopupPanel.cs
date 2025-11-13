using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.EventSystems;

namespace Littale {
    [RequireComponent(typeof(RectTransform))]
    public class PopupPanel : MonoBehaviour {

        [Header("UI Components")]
        public RectTransform container;
        public Button closeButton;

        [Header("Animation (PrimeTween)")]
        public float animationDuration = 0.3f;
        public Ease easeIn = Ease.OutBack;
        public Ease easeOut = Ease.InBack;

        [Header("Audio (from AudioManager)")]
        public string openSound = "popup_open";
        public string closeSound = "popup_close";

        Tween activeTween;
        Vector3 originalScale;

        void Awake() {
            if (closeButton != null) {
                closeButton.onClick.AddListener(Close);
            }

            originalScale = container.localScale;
            container.localScale = Vector3.zero;
            container.gameObject.SetActive(false);
        }

        public void Open() {
            if (activeTween.isAlive) {
                activeTween.Complete();
            }

            if (!string.IsNullOrEmpty(openSound) && SoundManager.Instance != null) {
                SoundManager.Instance.Play(openSound);
            }

            container.gameObject.SetActive(true);
            container.localScale = Vector3.zero;

            activeTween = Tween.Scale(container, Vector3.one, animationDuration, easeIn, useUnscaledTime: true)
                .OnComplete(() => {
                });

            EventSystem.current.SetSelectedGameObject(null);
        }

        public void Close() {
            if (activeTween.isAlive) {
                activeTween.Complete();
            }

            if (!string.IsNullOrEmpty(closeSound) && SoundManager.Instance != null) {
                SoundManager.Instance.Play(closeSound);
            }

            activeTween = Tween.Scale(container, Vector3.zero, animationDuration, easeOut, useUnscaledTime: true)
                .OnComplete(() => {
                    container.gameObject.SetActive(false);
                });
        }

    }
}