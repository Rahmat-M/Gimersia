using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    public class DeckSystem : MonoBehaviour {
        [Header("Auto Play Settings")]
        public bool autoPlay = true;
        public float delayBetweenCards = 1.0f; // Delay antar play kartu dalam hand
        private Coroutine autoPlayRoutine;
        public List<CardDataSO> startingDeck;
        private List<CardInstance> pile = new List<CardInstance>();
        private List<CardInstance> discard = new List<CardInstance>();
        private List<CardInstance> hand = new List<CardInstance>();
        public delegate void CardPlayedHandler(CardInstance card);
        public event CardPlayedHandler OnCardPlayed;
        public event System.Action<CardInstance, int> OnCardDrawn;
        public event System.Action<CardInstance> OnCardDiscarded;
        public int handSize = 3;
        private List<CardInstance> lastHandSnapshot = new List<CardInstance>();

        void Start() {
            // Delay hingga UI ready (fix first draw position bug)
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart() {
            Debug.Log("[DeckSystem] Waiting for UI to be ready...");
            yield return new WaitForSeconds(1.5f); // 1.5 detik cukup untuk Canvas stabilize. Adjust jika perlu (1-3s)
            Debug.Log("[DeckSystem] UI ready, initializing deck...");

            InitializeDeck();
            yield return StartCoroutine(DrawToFullHandCoroutine());
            if (autoPlay) StartAutoPlay();
        }

        void InitializeDeck() {
            pile.Clear();
            discard.Clear();
            hand.Clear();
            foreach (var cd in startingDeck)
                pile.Add(new CardInstance(cd));
            Shuffle(pile);
        }

        void Shuffle(List<CardInstance> list) {
            for (int i = 0; i < list.Count; i++) {
                int r = Random.Range(i, list.Count);
                var tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
        }

        private IEnumerator DrawToFullHandCoroutine() {
            int toDraw = Mathf.Max(0, handSize - hand.Count);
            for (int i = 0; i < toDraw; i++) {
                Draw(1);
                yield return new WaitForSeconds(0.02f); // Delay kecil antar draw anim
            }
        }

        public void Draw(int count) {
            for (int i = 0; i < count; i++) {
                if (pile.Count == 0)
                    RefillPileFromDiscard();
                if (pile.Count == 0) return;
                var top = pile[0];
                pile.RemoveAt(0);
                hand.Add(top);
                Debug.Log($"[DeckSystem] Drew {top.data.cardName} to hand index {hand.Count - 1}");
                OnCardDrawn?.Invoke(top, hand.Count - 1);
            }
        }

        public List<CardInstance> GetLastHandSnapshot() => lastHandSnapshot;
        public void StartAutoPlay() {
            if (autoPlayRoutine != null) StopCoroutine(autoPlayRoutine);
            autoPlayRoutine = StartCoroutine(AutoPlayLoop());
        }
        public void StopAutoPlay() {
            if (autoPlayRoutine != null) StopCoroutine(autoPlayRoutine);
            autoPlayRoutine = null;
        }
        private IEnumerator AutoPlayLoop() {
            Debug.Log("[DeckSystem] Auto play started");
            while (autoPlay) {
                if (hand.Count == 0)
                    yield return DrawToFullHandCoroutine();
                yield return PlayHandRoutine();
                // Optional: yield new WaitForSeconds(0.5f); jika butuh pause setelah discard sebelum draw next
            }
            Debug.Log("[DeckSystem] Auto play stopped");
        }
        void RefillPileFromDiscard() {
            if (discard.Count == 0) return;
            pile.AddRange(discard);
            discard.Clear();
            Shuffle(pile);
            Debug.Log("[DeckSystem] Refilled pile from discard");
        }
        public void DiscardCard(CardInstance card) {
            if (hand.Contains(card)) hand.Remove(card);
            discard.Add(card);
            OnCardDiscarded?.Invoke(card);
        }
        // Routine baru: Play semua kartu di hand, discard setelah semua selesai
        public IEnumerator PlayHandRoutine() {
            List<CardInstance> playedCards = new List<CardInstance>();
            // Play sequential
            while (hand.Count > 0) {
                var card = hand[0];
                hand.RemoveAt(0);
                Debug.Log($"[DeckSystem] Playing {card.data.cardName}");
                OnCardPlayed?.Invoke(card); // Efek + highlight UI
                playedCards.Add(card);
                yield return new WaitForSeconds(0.3f); // Wait highlight selesai
                yield return new WaitForSeconds(delayBetweenCards - 0.3f); // Sisa delay antar kartu
            }
            // Discard sequential setelah semua played
            foreach (var card in playedCards) {
                discard.Add(card);
                OnCardDiscarded?.Invoke(card); // Trigger anim discard
                                               //yield return new WaitForSeconds(0.2f); // Delay antar anim discard
            }
            lastHandSnapshot = new List<CardInstance>(hand);
        }
        public List<CardInstance> GetHand() => hand;
        public List<CardInstance> GetPile() => pile;
        public List<CardInstance> GetDiscard() => discard;
    }
}