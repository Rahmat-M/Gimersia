using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public class CardInventory : MonoBehaviour {

        // TODO: Add for Handling UI

        public UnityAction<ReactiveCardController> OnReactiveCardAcquired;
        public UnityAction<ActiveCardController> OnActiveCardAdded;

        [SerializeField] List<CardController> cardSlots = new List<CardController>();
        [SerializeField] List<PassiveCardController> passiveCardSlots = new List<PassiveCardController>();
        [SerializeField] ReactiveCardController reactionCardSlot;
        [SerializeField] ActiveCardController activeCardSlot;

        public bool Add(CardController card) {
            if (card == null) return false;
            cardSlots.Add(card);
            return true;
        }

        public bool Add(PassiveCardController card) {
            if (card == null) return false;
            passiveCardSlots.Add(card);
            return true;
        }

        public bool Add(ReactiveCardController card) {
            if (card == null) return false;

            if (HasReactiveCard()) {
                // TODO: Logic untuk menghancurkan/melempar GameObject kartu lama
            }

            reactionCardSlot = card;
            OnReactiveCardAcquired?.Invoke(card);
            return true;
        }

        public bool Add(ActiveCardController card) {
            if (card == null) return false;

            if (HasActiveCard()) {
                // TODO: Logic untuk menghancurkan/melempar GameObject kartu lama
            }

            activeCardSlot = card;
            OnActiveCardAdded?.Invoke(card);
            return true;
        }

        public bool Remove(CardController card) {
            return cardSlots.Remove(card);
        }

        public bool Remove(PassiveCardController card) {
            return passiveCardSlots.Remove(card);
        }

        public ReactiveCardController RemoveReactiveCard() {
            if (!HasReactiveCard()) return null;

            ReactiveCardController oldCard = reactionCardSlot;
            reactionCardSlot = null;
            return oldCard;
        }

        public ActiveCardController RemoveActiveCard() {
            if (!HasActiveCard()) return null;

            ActiveCardController oldCard = activeCardSlot;
            activeCardSlot = null;
            return oldCard;
        }

        public bool Has(CardController card) {
            return cardSlots.Contains(card);
        }

        public bool Has(PassiveCardController card) {
            return passiveCardSlots.Contains(card);
        }

        public bool HasReactiveCard() {
            return reactionCardSlot != null;
        }

        public bool HasActiveCard() {
            return activeCardSlot != null;
        }

        public IReadOnlyList<CardController> GetCardSlots() {
            return cardSlots;
        }

        public IReadOnlyList<PassiveCardController> GetPassiveCardSlots() {
            return passiveCardSlots;
        }

        public ReactiveCardController GetReactiveCard() {
            return reactionCardSlot;
        }

        public ActiveCardController GetActiveCard() {
            return activeCardSlot;
        }

        void OnDestroy() {
            OnReactiveCardAcquired = null;
            OnActiveCardAdded = null;
        }

    }
}