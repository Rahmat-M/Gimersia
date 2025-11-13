using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public class CardInventory : MonoBehaviour {

        public UnityAction<ReactiveCardController> OnReactiveCardAcquired;
        public UnityAction<ActiveCardController> OnActiveCardAdded;
        public UnityAction<PassiveCardController> OnPassiveCardAcquired;

        [SerializeField] List<CardController> cardSlots = new List<CardController>();
        [SerializeField] List<PassiveCardController> passiveCardSlots = new List<PassiveCardController>();
        [SerializeField] ReactiveCardController reactionCardSlot;
        [SerializeField] ActiveCardController activeCardSlot;

        public bool Add(CardController card) {
            if (card == null) return false;
            CardController spawnedCard = InstantiateCard(card);
            cardSlots.Add(spawnedCard);
            return true;
        }

        public bool Add(PassiveCardController card) {
            if (card == null) return false;
            PassiveCardController spawnedCard = InstantiateCard(card);
            passiveCardSlots.Add(spawnedCard);
            OnPassiveCardAcquired?.Invoke(spawnedCard);
            return true;
        }

        public bool Add(ReactiveCardController card) {
            if (card == null) return false;

            if (HasReactiveCard()) {
                Destroy(reactionCardSlot.gameObject);
                reactionCardSlot = null;
            }

            ReactiveCardController spawnedCard = InstantiateCard(card);
            reactionCardSlot = spawnedCard;
            OnReactiveCardAcquired?.Invoke(spawnedCard);
            return true;
        }

        public bool Add(ActiveCardController card) {
            if (card == null) return false;

            if (HasActiveCard()) {
                Destroy(activeCardSlot.gameObject);
                activeCardSlot = null;
            }

            ActiveCardController spawnedCard = InstantiateCard(card);
            activeCardSlot = spawnedCard;
            OnActiveCardAdded?.Invoke(spawnedCard);
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

        T InstantiateCard<T>(T card) where T : BaseCard {
            GameObject spawnedCard = Instantiate(card.gameObject, transform.position, Quaternion.identity, transform);
            return spawnedCard.GetComponent<T>();
        }

        void OnDestroy() {
            OnReactiveCardAcquired = null;
            OnActiveCardAdded = null;
        }

    }
}