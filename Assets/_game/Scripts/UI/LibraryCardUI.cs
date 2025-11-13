using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Littale {
    public class LibraryCardUI : MonoBehaviour {
        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] TextMeshProUGUI typeText;

        public void Initialize(BaseCardSO data) {
            if (data != null) {
                iconImage.sprite = data.icon;
                nameText.text = data.name;
                descriptionText.text = data.description;
                typeText.text = data.GetCardTypeString();
            }
        }

        public void SetupEmpty(string label) {
            nameText.text = label;
            iconImage.color = Color.clear;
        }
    }
}