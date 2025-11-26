using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class CanvasButtonAudioHandler : MonoBehaviour {
        [Header("Global Button Sounds")]
        public SfxID globalHoverSound = SfxID.UI_HoverSound;
        public SfxID globalClickSound = SfxID.UI_ClickSound;

        void Start() {
            Button[] buttons = GetComponentsInChildren<Button>(true);

            foreach (Button btn in buttons) {
                ButtonAudioHandler handler = btn.GetComponent<ButtonAudioHandler>();

                if (handler == null) {
                    handler = btn.gameObject.AddComponent<ButtonAudioHandler>();
                }

                if (handler.hoverSound != 0) handler.hoverSound = globalHoverSound;
                if (handler.clickSound != 0) handler.clickSound = globalClickSound;
            }
        }
    }
}
