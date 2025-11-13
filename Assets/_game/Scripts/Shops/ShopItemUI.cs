using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Littale {
    public class ShopItemUI : MonoBehaviour {
        [SerializeField] Image cardIcon;
        [SerializeField] TextMeshProUGUI cardNameText;
        [SerializeField] TextMeshProUGUI cardDescriptionText;
        [SerializeField] TextMeshProUGUI cardPriceText;
        [SerializeField] TextMeshProUGUI cardTypeText;
        [SerializeField] Button buyButton;

        BaseCardSO card;
        int price;
        ShopManager manager;

        public void Initialize(BaseCardSO cardData, int itemPrice, ShopManager shopManager) {
            card = cardData;
            price = itemPrice;
            manager = shopManager;

            cardIcon.sprite = cardData.icon;
            cardNameText.text = cardData.name;
            cardDescriptionText.text = cardData.description;
            cardTypeText.text = cardData.GetCardTypeString();
            cardPriceText.text = price.ToString();

            buyButton.onClick.AddListener(OnBuyClicked);
        }

        void OnBuyClicked() {
            manager.TryBuyCard(card, price);
        }

        public void SetAsSold() {
            buyButton.interactable = false;
            cardPriceText.text = "SOLD";
        }
    }
}