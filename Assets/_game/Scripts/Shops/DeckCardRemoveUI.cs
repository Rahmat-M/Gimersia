using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Littale {
    public class DeckCardRemoveUI : MonoBehaviour {
        [SerializeField] private Image cardIcon;
        [SerializeField] private Button removeButton;

        // private CardController cardInstance;
        private ShopManager manager;

        // public void Initialize(CardController card, ShopManager shopManager) {
        //     cardInstance = card;
        //     manager = shopManager;

        //     cardIcon.sprite = card.cardData.icon;

        //     removeButton.onClick.AddListener(OnRemoveClicked);
        // }

        // void OnRemoveClicked() {
        //     manager.TryRemoveCard(cardInstance);
        // }
    }
}