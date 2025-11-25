using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public class ShopManager : MonoBehaviour {
        public static ShopManager Instance;

        [Header("Data References")]
        [SerializeField] private List<BaseCardSO> mainDeckCardPool;
        [SerializeField] private List<BaseCardSO> rareCardPool;

        [Header("Cost Config")]
        public int baseCardPrice = 50;
        public int rareCardPrice = 200;
        public int refreshCost = 10;
        public int removeCardCost = 75;

        [Header("Systems References")]
        private PlayerCollector characterCollector;
        private CardInventory cardInventory;
        private ShopUI shopUI;
        private int currentWave = 0;

        public UnityAction<int> OnCurrencyChanged;

        private List<BaseCardSO> currentShopStock = new List<BaseCardSO>();

        void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Initialize(ShopUI ui) {
            shopUI = ui;
            characterCollector = FindFirstObjectByType<PlayerCollector>();
            cardInventory = FindFirstObjectByType<CardInventory>();

            OnCurrencyChanged?.Invoke(characterCollector.GetCoins());
        }

        public void OpenShop() {
            currentWave = SpawnManager.instance.currentWaveIndex + 1;
            GenerateShopStock();
            shopUI.ShowShop(currentShopStock);
        }

        void GenerateShopStock() {
            currentShopStock.Clear();

            for (int i = 0; i < 3; i++) {
                currentShopStock.Add(GetRandomCard(mainDeckCardPool));
            }

            if (currentWave > 5 && Random.Range(0, 100) < 25) { // 25% chance
                currentShopStock.Add(GetRandomCard(rareCardPool));
            }
        }

        private BaseCardSO GetRandomCard(List<BaseCardSO> pool) {
            return pool[Random.Range(0, pool.Count)];
        }

        public void TryBuyCard(BaseCardSO card, int price) {
            if (characterCollector.GetCoins() >= price) {
                characterCollector.SpendCoins(price);
                OnCurrencyChanged?.Invoke(characterCollector.GetCoins());

                GameObject cardInstance = Instantiate(card.Prefab);
                // Logika Add ke inventory berdasarkan tipe
                if (cardInstance.TryGetComponent<CardController>(out var mainCard))
                    cardInventory.Add(mainCard);
                else if (cardInstance.TryGetComponent<ReactiveCardController>(out var reactiveCard))
                    cardInventory.Add(reactiveCard);
                else if (cardInstance.TryGetComponent<ActiveCardController>(out var activeCard))
                    cardInventory.Add(activeCard);
                else if (cardInstance.TryGetComponent<PassiveCardController>(out var passiveCard))
                    cardInventory.Add(passiveCard);
                else
                    Debug.LogWarning("Card prefab does not have a recognized CardController component.");

                Debug.Log("Buying " + card.name);
                shopUI.MarkItemAsSold(card); // Update UI
            } else {
                Debug.Log("Not Enought Coins!");
                // TODO: feedback to the player
            }
        }

        public void TryRefreshShop() {
            if (characterCollector.GetCoins() >= refreshCost) {
                characterCollector.SpendCoins(refreshCost);
                OnCurrencyChanged?.Invoke(characterCollector.GetCoins());

                GenerateShopStock();
                shopUI.ShowShop(currentShopStock); // Refresh UI
            } else {
                Debug.Log("Not enough money to refresh!");
            }
        }

        public void ShowRemoveCardPanel() {
            shopUI.ShowRemovePanel(cardInventory.GetCardSlots());
        }

        public void TryRemoveCard(CardController card) {
            if (characterCollector.GetCoins() >= removeCardCost) {
                characterCollector.SpendCoins(removeCardCost);
                OnCurrencyChanged?.Invoke(characterCollector.GetCoins());

                cardInventory.Remove(card);
                Destroy(card.gameObject);

                shopUI.ShowRemovePanel(cardInventory.GetCardSlots());
            } else {
                Debug.Log("Not enough money!");
            }
        }
    }
}