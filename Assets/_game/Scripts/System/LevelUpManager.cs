using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Penting untuk '.Except()' dan '.OrderBy()'

namespace Littale {
    public class LevelUpManager : MonoBehaviour {

        public List<BaseCardSO> allCardsInGame;
        public int optionsToShow = 3;

        CharacterStats characterStats;
        CardInventory cardInventory;
        UICardChoiceWindow choiceWindow;

        void Awake() {
            characterStats = FindFirstObjectByType<CharacterStats>();
            cardInventory = FindFirstObjectByType<CardInventory>();
            choiceWindow = FindFirstObjectByType<UICardChoiceWindow>();
        }

        public void ShowLevelUpOptions() {
            Time.timeScale = 0f;

            List<BaseCardSO> availableOptions = GetAvailableCardOptions();

            List<BaseCardSO> choices = availableOptions
                                        .OrderBy(x => Random.value)
                                        .Take(optionsToShow)
                                        .ToList();

            if (choices.Count > 0) {
                choiceWindow.ShowChoices(choices, this);
            } else {
                Debug.LogWarning("There are no available cards to choose from!");
                CloseLevelUpWindow();
            }
        }

        public void OnCardChosen(BaseCardSO chosenCardData) {
            if (chosenCardData.Prefab == null) {
                Debug.LogError($"BaseCardSO '{chosenCardData.cardName}' does not have a valid prefab assigned.");
                CloseLevelUpWindow();
                return;
            }

            GameObject cardObject = Instantiate(chosenCardData.Prefab, transform.position, Quaternion.identity);
            cardObject.transform.parent = characterStats.transform;

            switch (chosenCardData.cardType) {
                case CardType.MainDeck:
                    if (cardObject.TryGetComponent(out CardController mainCard))
                        cardInventory.Add(mainCard);
                    break;
                case CardType.Passive:
                    if (cardObject.TryGetComponent(out PassiveCardController passiveCard))
                        cardInventory.Add(passiveCard);
                    break;
                case CardType.Reactive:
                    if (cardObject.TryGetComponent(out ReactiveCardController reactiveCard))
                        cardInventory.Add(reactiveCard);
                    break;
                case CardType.Active:
                    if (cardObject.TryGetComponent(out ActiveCardController activeCard))
                        cardInventory.Add(activeCard);
                    break;
            }

            // 3. Tutup jendela pilihan
            CloseLevelUpWindow();
        }

        public void CloseLevelUpWindow() {
            choiceWindow.gameObject.SetActive(false);
            Time.timeScale = 1f; // Lanjutkan permainan
        }

        private List<BaseCardSO> GetAvailableCardOptions() {
            HashSet<BaseCardSO> ownedCards = new HashSet<BaseCardSO>();

            foreach (var card in cardInventory.GetCardSlots())
                if (card.cardData != null) ownedCards.Add(card.cardData);

            foreach (PassiveCardController card in cardInventory.GetPassiveCardSlots())
                if (card.cardData != null) ownedCards.Add(card.cardData);

            var reactiveCard = cardInventory.GetReactiveCard();
            if (reactiveCard != null && reactiveCard.cardData != null)
                ownedCards.Add(reactiveCard.cardData);

            var activeCard = cardInventory.GetActiveCard();
            if (activeCard != null && activeCard.cardData != null)
                ownedCards.Add(activeCard.cardData);

            return allCardsInGame.Except(ownedCards).ToList();
        }

    }
}