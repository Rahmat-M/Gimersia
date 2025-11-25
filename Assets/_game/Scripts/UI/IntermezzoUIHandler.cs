using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class IntermezzoUIManager : MonoBehaviour {

        public GameObject shopPanel;
        public GameObject skipButton;

        public bool animatePopUp = true;

        void Start() {
            HideIntermezzoUI(true);

            Button btn = skipButton.GetComponent<Button>();
            if (btn) {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSkipClicked);
            }
        }

        public void ShowIntermezzoUI() {
            shopPanel.SetActive(true);
            skipButton.SetActive(true);

            if (animatePopUp) {
                shopPanel.transform.localScale = Vector3.zero;
                skipButton.transform.localScale = Vector3.zero;

                Tween.Scale(shopPanel.transform, Vector3.one, 0.3f, ease: Ease.OutBack);
                Tween.Scale(skipButton.transform, Vector3.one, 0.3f, ease: Ease.OutBack, startDelay: 0.1f);
            } else {
                shopPanel.transform.localScale = Vector3.one;
                skipButton.transform.localScale = Vector3.one;
            }
        }

        public void HideIntermezzoUI(bool immediate = false) {
            if (immediate || !animatePopUp) {
                shopPanel.SetActive(false);
                skipButton.SetActive(false);
                return;
            }

            Tween.Scale(shopPanel.transform, Vector3.zero, 0.3f, ease: Ease.InBack)
                .OnComplete(() => shopPanel.SetActive(false));

            Tween.Scale(skipButton.transform, Vector3.zero, 0.3f, ease: Ease.InBack)
                .OnComplete(() => skipButton.SetActive(false));
        }

        void OnSkipClicked() {
            if (SpawnManager.instance) {
                SpawnManager.instance.SkipIntermezzo();
            }
            HideIntermezzoUI();
        }

    }
}