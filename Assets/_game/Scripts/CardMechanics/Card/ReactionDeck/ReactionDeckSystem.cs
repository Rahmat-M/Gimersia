using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    public class ReactionDeckSystem : MonoBehaviour {
        [Header("References")]
        public PlayerHealth playerHealth;
        public PlayerCombat playerCombat;

        [Header("Reaction Deck Settings")]
        public int maxReactionSlots = 3;

        [Header("Starting Reaction Cards")]
        [Tooltip("Reaction cards yang otomatis didapat saat game start")]
        public List<CardDataSO> startingReactionCards = new List<CardDataSO>();

        [Tooltip("Delay sebelum reaction cards muncul (detik)")]
        public float reactionCardSpawnDelay = 2f;

        [Tooltip("Delay antar spawn reaction card (detik)")]
        public float delayBetweenReactionSpawns = 0.8f;

        // Reaction slots (persisten, tidak rotate)
        private List<ReactionCardSlot> reactionSlots = new List<ReactionCardSlot>();

        // Events
        public delegate void ReactionCardAddedHandler(CardDataSO card, int slotIndex);
        public event ReactionCardAddedHandler OnReactionCardAdded;

        public delegate void ReactionCardDrawingHandler(CardDataSO card, int slotIndex);
        public event ReactionCardDrawingHandler OnReactionCardDrawing; // Event untuk animasi draw

        public delegate void ReactionCardReplacedHandler(CardDataSO oldCard, CardDataSO newCard, int slotIndex);
        public event ReactionCardReplacedHandler OnReactionCardReplaced;

        public delegate void ReactionTriggeredHandler(CardDataSO card, int slotIndex);
        public event ReactionTriggeredHandler OnReactionTriggered;

        // Timer untuk TimeInterval trigger
        private float timeIntervalTimer = 0f;

        void Start() {
            // Subscribe ke player health events
            if (playerHealth != null) {
                playerHealth.OnHealthChanged += CheckHealthTriggers;
                playerHealth.OnDamaged += CheckDamageTriggers;
            }

            // Initialize dengan 1 slot kosong (akan diisi oleh starting cards)
            // Tidak perlu AddEmptySlot() karena akan otomatis terisi

            // Spawn starting reaction cards dengan delay
            StartCoroutine(SpawnStartingReactionCards());
        }

        IEnumerator SpawnStartingReactionCards() {
            // Tunggu beberapa detik setelah game start
            Debug.Log($"[ReactionDeck] Waiting {reactionCardSpawnDelay}s before spawning starting reaction cards...");
            yield return new WaitForSeconds(reactionCardSpawnDelay);

            // Spawn setiap starting reaction card dengan delay
            for (int i = 0; i < startingReactionCards.Count && i < maxReactionSlots; i++) {
                CardDataSO card = startingReactionCards[i];

                if (card == null) {
                    Debug.LogWarning($"[ReactionDeck] Starting reaction card at index {i} is null!");
                    continue;
                }

                if (!card.isReactionCard) {
                    Debug.LogWarning($"[ReactionDeck] {card.cardName} is not marked as reaction card!");
                    continue;
                }

                Debug.Log($"[ReactionDeck] Spawning starting reaction card: {card.cardName}");

                // Add slot baru
                var newSlot = new ReactionCardSlot { CardDataSO = card };
                reactionSlots.Add(newSlot);

                int slotIndex = reactionSlots.Count - 1;

                // Trigger event untuk animasi draw
                OnReactionCardDrawing?.Invoke(card, slotIndex);

                // Tunggu sebelum spawn card berikutnya
                yield return new WaitForSeconds(delayBetweenReactionSpawns);
            }

            Debug.Log($"[ReactionDeck] Finished spawning {reactionSlots.Count} starting reaction cards");
        }

        void Update() {
            // Update cooldowns
            foreach (var slot in reactionSlots) {
                if (slot.isOnCooldown) {
                    slot.cooldownTimer -= Time.deltaTime;
                    if (slot.cooldownTimer <= 0) {
                        slot.isOnCooldown = false;
                        Debug.Log($"[ReactionDeck] {slot.CardDataSO.cardName} cooldown finished");
                    }
                }
            }

            // Check time interval triggers
            timeIntervalTimer += Time.deltaTime;
            CheckTimeIntervalTriggers();
        }

        // Add reaction card (dipanggil saat player mendapat reaction card baru in-game)
        public void AddReactionCard(CardDataSO card) {
            if (!card.isReactionCard) {
                Debug.LogWarning($"[ReactionDeck] {card.cardName} is not a reaction card!");
                return;
            }

            // Cari slot kosong
            for (int i = 0; i < reactionSlots.Count; i++) {
                if (reactionSlots[i].CardDataSO == null) {
                    reactionSlots[i].CardDataSO = card;
                    reactionSlots[i].cooldownTimer = 0;
                    reactionSlots[i].isOnCooldown = false;

                    Debug.Log($"[ReactionDeck] Added {card.cardName} to slot {i}");

                    // Trigger animasi draw
                    OnReactionCardDrawing?.Invoke(card, i);
                    return;
                }
            }

            // Jika semua slot penuh, beri pilihan replace (di UI nanti)
            if (reactionSlots.Count < maxReactionSlots) {
                // Tambah slot baru
                var newSlot = new ReactionCardSlot { CardDataSO = card };
                reactionSlots.Add(newSlot);
                int slotIndex = reactionSlots.Count - 1;

                Debug.Log($"[ReactionDeck] Added {card.cardName} to new slot {slotIndex}");

                // Trigger animasi draw
                OnReactionCardDrawing?.Invoke(card, slotIndex);
            } else {
                Debug.Log($"[ReactionDeck] All slots full! Need to replace a card.");
                // Trigger UI untuk pilih kartu mana yang mau di-replace
                // Untuk sementara, replace slot pertama (implementasi UI nanti)
                ReplaceReactionCard(0, card);
            }
        }

        // Replace reaction card di slot tertentu
        public void ReplaceReactionCard(int slotIndex, CardDataSO newCard) {
            if (slotIndex < 0 || slotIndex >= reactionSlots.Count) {
                Debug.LogWarning($"[ReactionDeck] Invalid slot index: {slotIndex}");
                return;
            }

            CardDataSO oldCard = reactionSlots[slotIndex].CardDataSO;
            reactionSlots[slotIndex].CardDataSO = newCard;
            reactionSlots[slotIndex].cooldownTimer = 0;
            reactionSlots[slotIndex].isOnCooldown = false;

            Debug.Log($"[ReactionDeck] Replaced {oldCard?.cardName} with {newCard.cardName} in slot {slotIndex}");
            OnReactionCardReplaced?.Invoke(oldCard, newCard, slotIndex);
        }

        // Check triggers berdasarkan health percentage
        void CheckHealthTriggers(float currentHP, float maxHP, float percentage) {
            for (int i = 0; i < reactionSlots.Count; i++) {
                var slot = reactionSlots[i];
                if (slot.CardDataSO == null || slot.isOnCooldown) continue;

                if (slot.CardDataSO.triggerType == ReactionTriggerType.HealthBelow) {
                    if (percentage < slot.CardDataSO.triggerThreshold) {
                        TriggerReaction(i);
                    }
                } else if (slot.CardDataSO.triggerType == ReactionTriggerType.HealthAbove) {
                    if (percentage > slot.CardDataSO.triggerThreshold) {
                        TriggerReaction(i);
                    }
                }
            }
        }

        // Check triggers saat player damaged
        void CheckDamageTriggers(float damageAmount) {
            for (int i = 0; i < reactionSlots.Count; i++) {
                var slot = reactionSlots[i];
                if (slot.CardDataSO == null || slot.isOnCooldown) continue;

                if (slot.CardDataSO.triggerType == ReactionTriggerType.OnDamaged) {
                    TriggerReaction(i);
                }
            }
        }

        // Check triggers berdasarkan time interval
        void CheckTimeIntervalTriggers() {
            for (int i = 0; i < reactionSlots.Count; i++) {
                var slot = reactionSlots[i];
                if (slot.CardDataSO == null || slot.isOnCooldown) continue;

                if (slot.CardDataSO.triggerType == ReactionTriggerType.TimeInterval) {
                    if (timeIntervalTimer >= slot.CardDataSO.triggerThreshold) {
                        TriggerReaction(i);
                        timeIntervalTimer = 0;
                    }
                }
            }
        }

        // Trigger reaction card effect
        void TriggerReaction(int slotIndex) {
            var slot = reactionSlots[slotIndex];
            if (slot.CardDataSO == null) return;

            Debug.Log($"[ReactionDeck] Triggering {slot.CardDataSO.cardName} from slot {slotIndex}");

            // Execute card effect
            ExecuteReactionEffect(slot.CardDataSO);

            // Set cooldown
            slot.isOnCooldown = true;
            slot.cooldownTimer = slot.CardDataSO.cooldown;

            // Trigger event untuk UI
            OnReactionTriggered?.Invoke(slot.CardDataSO, slotIndex);
        }

        // Execute reaction card effect
        void ExecuteReactionEffect(CardDataSO card) {
            switch (card.cardType) {
                case CardType.Recovery:
                    if (playerHealth != null) {
                        playerHealth.Heal(card.healAmount);
                        Debug.Log($"[ReactionDeck] Healed {card.healAmount} HP");
                    }
                    break;

                case CardType.Defense:
                    if (playerHealth != null) {
                        playerHealth.AddShield(card.defenseAmount, card.effectDuration);
                        Debug.Log($"[ReactionDeck] Added {card.defenseAmount} shield for {card.effectDuration}s");
                    }
                    break;

                case CardType.Buff:
                    // Implement buff effects (speed boost, damage boost, etc)
                    StartCoroutine(ApplyBuff(card));
                    break;
            }
        }

        IEnumerator ApplyBuff(CardDataSO card) {
            Debug.Log($"[ReactionDeck] Applied buff: {card.cardName} for {card.effectDuration}s");
            // Implement buff logic here
            // Example: increase move speed, damage, etc
            yield return new WaitForSeconds(card.effectDuration);
            Debug.Log($"[ReactionDeck] Buff {card.cardName} expired");
        }

        // Helper method untuk add empty slot
        void AddEmptySlot() {
            if (reactionSlots.Count < maxReactionSlots) {
                reactionSlots.Add(new ReactionCardSlot());
            }
        }

        // Public getters
        public List<ReactionCardSlot> GetReactionSlots() => reactionSlots;
        public int GetAvailableSlots() => maxReactionSlots - reactionSlots.Count;
        public bool HasEmptySlot() {
            foreach (var slot in reactionSlots)
                if (slot.CardDataSO == null) return true;
            return false;
        }
    }

    // Class untuk menyimpan reaction card slot data
    [System.Serializable]
    public class ReactionCardSlot {
        public CardDataSO CardDataSO;
        public bool isOnCooldown;
        public float cooldownTimer;
    }
}