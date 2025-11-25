using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public class DeckManager : MonoBehaviour {
        [Header("Revolver System")]
        public CardData slot1; // Slot Aktif 1 (Tombol Q)
        public CardData slot2; // Slot Aktif 2 (Tombol E)

        [Header("Queue & Cycle")]
        // Tumpukan kartu skill yang menunggu giliran
        public List<CardData> skillDeckQueue = new List<CardData>();
        public List<CardData> skillDiscard = new List<CardData>();

        // Events untuk Update UI
        public UnityEvent<CardData> OnSlot1Changed;
        public UnityEvent<CardData> OnSlot2Changed;

        private PlayerCardManager playerCardManager;

        void Start() {
            playerCardManager = GetComponent<PlayerCardManager>();
            InitializeDeck();
        }

        void InitializeDeck() {
            // Ambil kartu Revolver dari Inventory atau sumber lain
            // skillDeckQueue = inventory.GetRevolverCards();

            Shuffle(skillDeckQueue);

            FillSlot(1);
            FillSlot(2);
        }

        public void TryUseCard(int slotIndex) {
            CardData cardToUse = (slotIndex == 1) ? slot1 : slot2;

            if (cardToUse == null) return;

            if (playerCardManager.CurrentMana >= cardToUse.manaCost) {
                // 1. Gunakan Kartu
                playerCardManager.UseCard(cardToUse);
                playerCardManager.ConsumeMana(cardToUse.manaCost);

                // 2. Buang ke Discard
                skillDiscard.Add(cardToUse);

                // 3. Kosongkan Slot
                if (slotIndex == 1) slot1 = null;
                else slot2 = null;

                // 4. Isi ulang slot dari Queue (Revolver reload)
                StartCoroutine(ReloadSlotDelay(slotIndex, 0.5f));
            } else {
                Debug.Log("Not Enough Mana!");
                // UI Feedback: Flash Mana Bar Merah
            }
        }

        IEnumerator ReloadSlotDelay(int slotIndex, float delay) {
            yield return new WaitForSeconds(delay);
            FillSlot(slotIndex);
        }

        void FillSlot(int slotIndex) {
            if (skillDeckQueue.Count == 0) {
                ReshuffleDiscardToQueue();
            }

            if (skillDeckQueue.Count > 0) {
                CardData newCard = skillDeckQueue[0];
                skillDeckQueue.RemoveAt(0);

                if (slotIndex == 1) {
                    slot1 = newCard;
                    OnSlot1Changed?.Invoke(slot1);
                } else {
                    slot2 = newCard;
                    OnSlot2Changed?.Invoke(slot2);
                }
            }
        }

        void ReshuffleDiscardToQueue() {
            if (skillDiscard.Count == 0) return;
            skillDeckQueue.AddRange(skillDiscard);
            skillDiscard.Clear();
            Shuffle(skillDeckQueue);
        }

        void Shuffle(List<CardData> list) {
            for (int i = 0; i < list.Count; i++) {
                CardData temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}