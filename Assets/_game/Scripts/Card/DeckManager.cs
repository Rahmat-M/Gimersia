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

        public void TryUseCard(int slotIndex, bool isHold = false) {
            CardData cardToUse = (slotIndex == 1) ? slot1 : slot2;

            if (cardToUse == null) return;

            if (playerCardManager.CurrentMana >= cardToUse.manaCost) {
                // 1. Gunakan Kartu
                playerCardManager.UseCard(cardToUse, CardTier.Bronze, slotIndex);
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

        public void ResetCooldown(int slotIndex) {
            // Logic to reset cooldown for the specific slot
            // Since the current system uses a queue and immediate use, 
            // "Reset Cooldown" might mean "Instantly refill the slot" if it's empty 
            // or "Allow immediate use again" if there's a timer.
            
            // For this specific implementation where slots are refilled from a queue:
            // If the slot is empty (waiting for reload), we can force fill it immediately.
            // Or if there is a cooldown timer on the UI (which we saw in ActiveDeckHandler), we reset that.
            
            // Assuming the "Cooldown" is the reload delay:
            StopAllCoroutines(); // Stop any reload delay
            FillSlot(slotIndex); // Fill immediately
            Debug.Log($"Cooldown Reset for Slot {slotIndex}!");
        }

        public void AddCard(CardData newCard) {
            // Check for fusion
            if (newCard.nextTierCard != null) {
                // Check Queue
                CardData match = skillDeckQueue.Find(x => x.cardName == newCard.cardName && x.tier == newCard.tier);
                if (match != null) {
                    FuseCards(match, newCard);
                    return;
                }

                // Check Discard
                match = skillDiscard.Find(x => x.cardName == newCard.cardName && x.tier == newCard.tier);
                if (match != null) {
                    skillDiscard.Remove(match);
                    FuseCards(null, newCard); // Null match means we just add the upgraded one
                    return;
                }

                // Check Active Slots (If we want to allow fusing with active cards - might be complex mid-combat)
                // For simplicity, let's say we only fuse with Queue/Discard for now, 
                // OR we can swap the active slot if it matches.
                if (slot1 != null && slot1.cardName == newCard.cardName && slot1.tier == newCard.tier) {
                    slot1 = null; // Clear slot
                    FuseCards(null, newCard);
                    FillSlot(1); // Refill slot
                    return;
                }
                if (slot2 != null && slot2.cardName == newCard.cardName && slot2.tier == newCard.tier) {
                    slot2 = null;
                    FuseCards(null, newCard);
                    FillSlot(2);
                    return;
                }
            }

            // No fusion, just add
            skillDeckQueue.Add(newCard);
            Shuffle(skillDeckQueue);
        }

        void FuseCards(CardData existingCard, CardData newCard) {
            if (existingCard != null) skillDeckQueue.Remove(existingCard);
            
            CardData upgradedCard = newCard.nextTierCard;
            Debug.Log($"FUSION! {newCard.cardName} ({newCard.tier}) -> {upgradedCard.cardName} ({upgradedCard.tier})");
            
            // Apply Player Stats Bonus
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats) stats.ApplyFusionBonus((int)upgradedCard.tier); // 1=Silver, 2=Gold (Enum index might vary, check CardTier)

            // Recursively check if we can fuse the NEW upgraded card
            AddCard(upgradedCard);
        }

        public bool RemoveCard(CardData card) {
            // Try removing from Queue
            CardData match = skillDeckQueue.Find(x => x.cardName == card.cardName && x.tier == card.tier);
            if (match != null) {
                skillDeckQueue.Remove(match);
                return true;
            }

            // Try removing from Discard
            match = skillDiscard.Find(x => x.cardName == card.cardName && x.tier == card.tier);
            if (match != null) {
                skillDiscard.Remove(match);
                return true;
            }

            return false;
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