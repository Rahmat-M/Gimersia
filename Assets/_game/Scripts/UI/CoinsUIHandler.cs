using UnityEngine;

namespace Littale {
    public class CoinsUIHandler : MonoBehaviour {

        [SerializeField] TMPro.TMP_Text coinsDisplay;
        [SerializeField] CharacterCollector characterCollector;

        void Start() {
            if (characterCollector != null) {
                characterCollector.OnCoinCollected.AddListener(UpdateCoinsDisplay);
                UpdateCoinsDisplay(characterCollector.GetCoins());
            }
        }

        void UpdateCoinsDisplay(float amount) {
            if (coinsDisplay != null) {
                coinsDisplay.text = amount.ToString("0");
            }
        }

    }
}