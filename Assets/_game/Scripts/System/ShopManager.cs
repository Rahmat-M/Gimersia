using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class ShopManager : MonoBehaviour {
        [Header("Shop Settings")]
        public List<CardData> commonCards;
        public List<CardData> rareCards;
        public List<CardData> legendaryCards;

        [Header("Prices")]
        public int priceCommon = 100;
        public int priceRare = 250;
        public int priceLegendary = 500;
        public float sellMultiplier = 0.3f;

        public void BuyCard(CardData card) {
            PlayerCollector collector = FindFirstObjectByType<PlayerCollector>();
            int price = GetPrice(card);

            if (collector && collector.GetCoins() >= price) {
                collector.SpendCoins(price);
                FindFirstObjectByType<DeckManager>().AddCard(card);
                Debug.Log($"Bought {card.cardName} for {price}G");
            } else {
                Debug.Log("Not enough Gold!");
            }
        }

        public void SellCard(CardData card) {
            DeckManager deck = FindFirstObjectByType<DeckManager>();
            if (deck.RemoveCard(card)) {
                int sellValue = Mathf.RoundToInt(GetPrice(card) * sellMultiplier);
                FindFirstObjectByType<PlayerCollector>().AddCoins(sellValue);
                Debug.Log($"Sold {card.cardName} for {sellValue}G");
            }
        }

        public float discountMultiplier = 1.0f; // For Art Sale passive

        int GetPrice(CardData card) {
            int basePrice = 100;
            switch (card.tier) {
                case CardTier.Bronze: basePrice = 100; break;
                case CardTier.Silver: basePrice = 250; break;
                case CardTier.Gold: basePrice = 600; break;
            }
            return Mathf.RoundToInt(basePrice * discountMultiplier);
        }
    }
}
