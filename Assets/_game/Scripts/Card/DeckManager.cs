using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public class CardInstance {
        public CardController data;
        public CardInstance(CardController d) { data = d; }
    }

    public class DeckManager : MonoBehaviour {

        public UnityAction<CardController, int> OnCardDrawn;
        public UnityAction<CardController, float> OnCardPlayed;
        public UnityAction<CardController> OnCardDiscarded;

        [SerializeField] bool autoStart = true;

        [Header("Deck Settings")]
        [SerializeField] float minCardDelay = 0.2f; // Minimum delay between playing cards
        [SerializeField] bool isPlaying;

        float delayBetweenCards = 1f;
        CharacterStats characterStats;
        CardInventory cardInventory;

        // Cards
        List<CardController> pile = new List<CardController>();
        List<CardController> hand = new List<CardController>();
        List<CardController> discard = new List<CardController>();

        // Cache
        Coroutine autoPlayCoroutine;
        WaitForSeconds waitDelayBetweenCards;

        void Awake() {
            waitDelayBetweenCards = new WaitForSeconds(delayBetweenCards);
            autoPlayCoroutine = null;
        }

        void Start() {
            characterStats = FindFirstObjectByType<CharacterStats>();
            cardInventory = characterStats.Inventory;
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart() {
            yield return waitDelayBetweenCards; // Wait for UI to be ready

            float calculatedDelay = 1f / characterStats.Actual.attackSpeed;
            delayBetweenCards = Mathf.Max(minCardDelay, calculatedDelay);
            waitDelayBetweenCards = new WaitForSeconds(delayBetweenCards);

            InitializeDeck();
            yield return DrawToFullHandCoroutine();
            if (autoStart) StartAutoPlay();
        }

        void InitializeDeck() {
            pile.Clear();
            discard.Clear();
            hand.Clear();

            foreach (var cd in cardInventory.GetCardSlots()) {
                pile.Add(cd);
            }

            Shuffle(pile);
        }

        IEnumerator DrawToFullHandCoroutine() {
            int handSize = characterStats.Actual.handSize;
            int toDraw = Mathf.Max(0, handSize - hand.Count);
            for (int i = 0; i < toDraw; i++) {
                Draw(1);
                yield return new WaitForSeconds(0.02f); // Small delay between draw animations
            }
        }

        IEnumerator AutoPlayLoopCoroutine() {
            while (isPlaying) {
                if (hand.Count == 0) yield return DrawToFullHandCoroutine();

                yield return PlayHandCoroutine();
            }
        }

        public void StartAutoPlay() {
            isPlaying = true;
            if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = StartCoroutine(AutoPlayLoopCoroutine());
        }

        public void StopAutoPlay() {
            if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
            isPlaying = false;
        }

        public IEnumerator PlayHandCoroutine() {
            while (hand.Count > 0) {
                var cardToPlay = hand[0];
                hand.RemoveAt(0);
                cardToPlay.Attack(); // Execute card effect
                OnCardPlayed?.Invoke(cardToPlay, delayBetweenCards); // Notify UI
                discard.Add(cardToPlay);
                yield return waitDelayBetweenCards;
            }
        }

        public void Draw(int count) {
            for (int i = 0; i < count; i++) {
                if (pile.Count == 0) {
                    RefillPileFromDiscard();
                }

                if (pile.Count == 0) { // Should not happen after refill
                    Debug.LogWarning("[DeckManager] No cards left to draw!");
                    return;
                }

                // Draw top card from pile to hand
                var top = pile[0];
                pile.RemoveAt(0);
                hand.Add(top);

                OnCardDrawn?.Invoke(top, hand.Count - 1); // Notify UI
            }
        }

        public void Discard(CardController card) {
            if (hand.Remove(card)) {
                discard.Add(card);
                OnCardDiscarded?.Invoke(card); // Notify UI
            } else {
                Debug.LogWarning("[DeckManager] Tried to discard a card not in hand!");
            }
        }

        public void RefillPileFromDiscard() {
            if (discard.Count == 0) return;

            pile.AddRange(discard);
            discard.Clear();
            SoundManager.Instance.Play("card_shuffled");
            Shuffle(pile);
        }

        public void Shuffle(List<CardController> list) { // Fisher-Yates shuffle
            for (int i = 0; i < list.Count; i++) {
                int r = Random.Range(i, list.Count);
                var tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
        }

        public void AddCardToPile(CardController card) {
            pile.Add(card);
        }

        public List<CardController> GetHand() => hand;
        public List<CardController> GetPile() => pile;
        public List<CardController> GetDiscard() => discard;

        void OnDestroy() {
            StopAutoPlay();
        }

#if UNITY_EDITOR
        [ContextMenu("Play The Deck")]
        void PlayTheDeck() {
            StartAutoPlay();
        }

        [ContextMenu("Stop The Deck")]
        void StopTheDeck() {
            StopAutoPlay();
        }
#endif

    }
}