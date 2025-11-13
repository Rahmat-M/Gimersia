using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Littale {
    public class ShopUI : MonoBehaviour {
        [Header("Main")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private Button closeButton;

        [Header("Item List")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject shopItemPrefab;

        [Header("Buttons")]
        [SerializeField] private Button refreshButton;
        [SerializeField] private TextMeshProUGUI refreshCostText;
        [SerializeField] private Button removeCardButton;
        [SerializeField] private TextMeshProUGUI removeCostText;

        [Header("Remove Card Panel")]
        [SerializeField] private GameObject removeCardPanel;
        [SerializeField] private Transform removeCardContainer;
        [SerializeField] private GameObject deckCardRemovePrefab;
        [SerializeField] private Button closeRemovePanelButton;

        private ShopManager manager;
        private Dictionary<BaseCardSO, ShopItemUI> spawnedItems = new Dictionary<BaseCardSO, ShopItemUI>();

        void Start() {
            manager = ShopManager.Instance;
            manager.Initialize(this);

            closeButton.onClick.AddListener(CloseShop);
            refreshButton.onClick.AddListener(manager.TryRefreshShop);
            removeCardButton.onClick.AddListener(manager.ShowRemoveCardPanel);
            closeRemovePanelButton.onClick.AddListener(() => removeCardPanel.SetActive(false));

            manager.OnCurrencyChanged += UpdateCurrencyText;

            refreshCostText.text = $"({manager.refreshCost}G)";
            removeCostText.text = $"({manager.removeCardCost}G)";

            shopPanel.SetActive(false);
            removeCardPanel.SetActive(false);
        }

        void UpdateCurrencyText(int amount) {
            currencyText.text = amount.ToString();
        }

        public void ShowShop(List<BaseCardSO> stock) {
            shopPanel.SetActive(true);

            foreach (Transform child in itemContainer) {
                Destroy(child.gameObject);
            }
            spawnedItems.Clear();

            foreach (BaseCardSO card in stock) {
                GameObject itemObj = Instantiate(shopItemPrefab, itemContainer);
                ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();

                bool isRare = card is ReactiveCardSO || card is ActiveCardSO;
                int price = isRare ? manager.rareCardPrice : manager.baseCardPrice;

                itemUI.Initialize(card, price, manager);
                spawnedItems[card] = itemUI;
            }
        }

        public void CloseShop() {
            shopPanel.SetActive(false);
            removeCardPanel.SetActive(false);
        }

        public void MarkItemAsSold(BaseCardSO card) {
            if (spawnedItems.TryGetValue(card, out ShopItemUI itemUI)) {
                itemUI.SetAsSold();
            }
        }

        public void ShowRemovePanel(IReadOnlyList<CardController> playerDeck) {
            removeCardPanel.SetActive(true);

            foreach (Transform child in removeCardContainer) {
                Destroy(child.gameObject);
            }

            foreach (CardController card in playerDeck) {
                GameObject cardObj = Instantiate(deckCardRemovePrefab, removeCardContainer);
                cardObj.GetComponent<DeckCardRemoveUI>().Initialize(card, manager);
            }
        }
    }
}