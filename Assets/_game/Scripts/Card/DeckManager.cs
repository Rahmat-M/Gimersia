using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
        public System.Action<List<CardData>> OnQueueChanged;
        public UnityEvent<float> OnReloadStart;

        private PlayerCardManager playerCardManager;

        [Header("Settings")]
        public float reloadCooldown = 1.0f;
        private bool isReloading = false;

        void Start() {
            playerCardManager = GetComponent<PlayerCardManager>();
            InitializeDeck();
        }

        public void OnReload(InputAction.CallbackContext context) {
            if (context.performed) {
                ManualReload();
            }
        }

        void InitializeDeck() {
            // Ambil kartu Revolver dari Inventory atau sumber lain
            // skillDeckQueue = inventory.GetRevolverCards();

            Shuffle(skillDeckQueue);

            FillSlot(1);
            FillSlot(2);
        }

        public void TryUseCard(int slotIndex, bool isHold = false) {
            if (isReloading) return;

            CardData cardToUse = (slotIndex == 1) ? slot1 : slot2;

            if (cardToUse == null) return;

            if (playerCardManager.CurrentMana >= cardToUse.manaCost) {
                // 1. Gunakan Kartu
                playerCardManager.UseCard(cardToUse, CardTier.Bronze, slotIndex);
                playerCardManager.ConsumeMana(cardToUse.manaCost);

                // 2. Buang ke Discard
                skillDiscard.Add(cardToUse);

                // 3. Kosongkan Slot
                if (slotIndex == 1) {
                    slot1 = null;
                    OnSlot1Changed?.Invoke(null);
                } else {
                    slot2 = null;
                    OnSlot2Changed?.Invoke(null);
                }

                // 4. Cek Reload Logic
                if (skillDeckQueue.Count > 0) {
                    // Masih ada kartu di queue, isi slot seperti biasa
                    StartCoroutine(ReloadSlotDelay(slotIndex, 0.5f));
                } else {
                    // Queue kosong, cek apakah kedua slot kosong
                    if (slot1 == null && slot2 == null) {
                        StartCoroutine(ReloadDeckRoutine());
                    }
                }

            } else {
                Debug.Log("Not Enough Mana!");
                // UI Feedback: Flash Mana Bar Merah
            }
        }

        IEnumerator ReloadSlotDelay(int slotIndex, float delay) {
            yield return new WaitForSeconds(delay);
            FillSlot(slotIndex);
        }

        IEnumerator ReloadDeckRoutine() {
            isReloading = true;
            Debug.Log("Reloading Deck...");

            OnReloadStart?.Invoke(reloadCooldown);

            yield return new WaitForSeconds(reloadCooldown);

            ReshuffleDiscardToQueue();

            FillSlot(1);
            FillSlot(2);

            isReloading = false;
            Debug.Log("Reload Complete!");
        }

        public void ManualReload() {
            if (isReloading) return;

            // Discard current cards
            if (slot1 != null) {
                skillDiscard.Add(slot1);
                slot1 = null;
                OnSlot1Changed?.Invoke(null);
            }
            if (slot2 != null) {
                skillDiscard.Add(slot2);
                slot2 = null;
                OnSlot2Changed?.Invoke(null);
            }

            skillDiscard.AddRange(skillDeckQueue);
            skillDeckQueue.Clear();
            OnQueueChanged?.Invoke(skillDeckQueue);

            StartCoroutine(ReloadDeckRoutine());
        }

        void FillSlot(int slotIndex) {
            // Removed automatic ReshuffleDiscardToQueue here.
            // Only fill if there are cards in Queue.

            if (skillDeckQueue.Count > 0) {
                CardData newCard = skillDeckQueue[0];
                skillDeckQueue.RemoveAt(0);
                OnQueueChanged?.Invoke(skillDeckQueue);

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
            if (skillDiscard.Count == 0 && skillDeckQueue.Count == 0) return; // Nothing to shuffle

            // If we want to reshuffle everything (including what was in queue if we did a manual reload with cards left in queue)
            // But usually Reshuffle is Discard -> Queue.
            // If Manual Reload was called, we might have cards in Queue?
            // If so, we should probably keep them or shuffle them too?
            // Let's just append Discard to Queue and Shuffle Queue.

            skillDeckQueue.AddRange(skillDiscard);
            skillDiscard.Clear();
            Shuffle(skillDeckQueue);
            OnQueueChanged?.Invoke(skillDeckQueue);
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
            OnQueueChanged?.Invoke(skillDeckQueue);
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
                OnQueueChanged?.Invoke(skillDeckQueue);
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