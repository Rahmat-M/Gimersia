using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Littale {
    public class LevelUpManager : MonoBehaviour {

        public List<BaseCardSO> allCardsInGame;
        public int optionsToShow = 3;

        CharacterStats characterStats;
        CardInventory cardInventory;
        DeckManager deckManager;
        public UICardChoiceWindow choiceWindow;

        void Awake() {
            characterStats = FindFirstObjectByType<CharacterStats>();
            cardInventory = FindFirstObjectByType<CardInventory>();
            deckManager = FindFirstObjectByType<DeckManager>();
        }

        public void ShowLevelUpOptions() {
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

            GameObject cardObject = Instantiate(chosenCardData.Prefab, characterStats.transform.position, Quaternion.identity);
            cardObject.transform.parent = characterStats.transform;

            switch (chosenCardData.cardType) {
                case CardType.MainDeck:
                    if (cardObject.TryGetComponent(out CardController mainCard)) {
                        cardInventory.Add(mainCard);
                        deckManager.AddCardToPile(mainCard);
                    }
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

            CloseLevelUpWindow();
        }

        public void CloseLevelUpWindow() {
            choiceWindow.gameObject.SetActive(false);
            GameManager.Instance.EndLevelUp();
        }

        private List<BaseCardSO> GetAvailableCardOptions() {
            HashSet<BaseCardSO> cardsToExclude = new HashSet<BaseCardSO>();

            if (cardInventory.HasReactiveCard()) {
                foreach (var card in allCardsInGame) {
                    if (card.cardType == CardType.Reactive) cardsToExclude.Add(card);
                }
            }

            if (cardInventory.HasActiveCard()) {
                foreach (var card in allCardsInGame) {
                    if (card.cardType == CardType.Active) cardsToExclude.Add(card);
                }
            }

            return allCardsInGame.Except(cardsToExclude).ToList();
        }

    }
}