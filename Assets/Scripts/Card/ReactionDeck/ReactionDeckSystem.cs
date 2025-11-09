using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionDeckSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;

    [Header("Reaction Deck Settings")]
    public int maxReactionSlots = 3;

    // Reaction slots (persisten, tidak rotate)
    private List<ReactionCardSlot> reactionSlots = new List<ReactionCardSlot>();

    // Events
    public delegate void ReactionCardAddedHandler(CardData card, int slotIndex);
    public event ReactionCardAddedHandler OnReactionCardAdded;

    public delegate void ReactionCardReplacedHandler(CardData oldCard, CardData newCard, int slotIndex);
    public event ReactionCardReplacedHandler OnReactionCardReplaced;

    public delegate void ReactionTriggeredHandler(CardData card, int slotIndex);
    public event ReactionTriggeredHandler OnReactionTriggered;

    // Timer untuk TimeInterval trigger
    private float timeIntervalTimer = 0f;

    void Start()
    {
        // Subscribe ke player health events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += CheckHealthTriggers;
            playerHealth.OnDamaged += CheckDamageTriggers;
        }

        // Initialize dengan 1 slot kosong
        AddEmptySlot();
    }

    void Update()
    {
        // Update cooldowns
        foreach (var slot in reactionSlots)
        {
            if (slot.isOnCooldown)
            {
                slot.cooldownTimer -= Time.deltaTime;
                if (slot.cooldownTimer <= 0)
                {
                    slot.isOnCooldown = false;
                    Debug.Log($"[ReactionDeck] {slot.cardData.cardName} cooldown finished");
                }
            }
        }

        // Check time interval triggers
        timeIntervalTimer += Time.deltaTime;
        CheckTimeIntervalTriggers();
    }

    // Add reaction card (dipanggil saat player mendapat reaction card baru)
    public void AddReactionCard(CardData card)
    {
        if (!card.isReactionCard)
        {
            Debug.LogWarning($"[ReactionDeck] {card.cardName} is not a reaction card!");
            return;
        }

        // Cari slot kosong
        for (int i = 0; i < reactionSlots.Count; i++)
        {
            if (reactionSlots[i].cardData == null)
            {
                reactionSlots[i].cardData = card;
                reactionSlots[i].cooldownTimer = 0;
                reactionSlots[i].isOnCooldown = false;

                Debug.Log($"[ReactionDeck] Added {card.cardName} to slot {i}");
                OnReactionCardAdded?.Invoke(card, i);
                return;
            }
        }

        // Jika semua slot penuh, beri pilihan replace (di UI nanti)
        if (reactionSlots.Count < maxReactionSlots)
        {
            // Tambah slot baru
            var newSlot = new ReactionCardSlot { cardData = card };
            reactionSlots.Add(newSlot);
            Debug.Log($"[ReactionDeck] Added {card.cardName} to new slot {reactionSlots.Count - 1}");
            OnReactionCardAdded?.Invoke(card, reactionSlots.Count - 1);
        }
        else
        {
            Debug.Log($"[ReactionDeck] All slots full! Need to replace a card.");
            // Trigger UI untuk pilih kartu mana yang mau di-replace
            // Untuk sementara, replace slot pertama (implementasi UI nanti)
            ReplaceReactionCard(0, card);
        }
    }

    // Replace reaction card di slot tertentu
    public void ReplaceReactionCard(int slotIndex, CardData newCard)
    {
        if (slotIndex < 0 || slotIndex >= reactionSlots.Count)
        {
            Debug.LogWarning($"[ReactionDeck] Invalid slot index: {slotIndex}");
            return;
        }

        CardData oldCard = reactionSlots[slotIndex].cardData;
        reactionSlots[slotIndex].cardData = newCard;
        reactionSlots[slotIndex].cooldownTimer = 0;
        reactionSlots[slotIndex].isOnCooldown = false;

        Debug.Log($"[ReactionDeck] Replaced {oldCard?.cardName} with {newCard.cardName} in slot {slotIndex}");
        OnReactionCardReplaced?.Invoke(oldCard, newCard, slotIndex);
    }

    // Check triggers berdasarkan health percentage
    void CheckHealthTriggers(float currentHP, float maxHP, float percentage)
    {
        for (int i = 0; i < reactionSlots.Count; i++)
        {
            var slot = reactionSlots[i];
            if (slot.cardData == null || slot.isOnCooldown) continue;

            if (slot.cardData.triggerType == ReactionTriggerType.HealthBelow)
            {
                if (percentage < slot.cardData.triggerThreshold)
                {
                    TriggerReaction(i);
                }
            }
            else if (slot.cardData.triggerType == ReactionTriggerType.HealthAbove)
            {
                if (percentage > slot.cardData.triggerThreshold)
                {
                    TriggerReaction(i);
                }
            }
        }
    }

    // Check triggers saat player damaged
    void CheckDamageTriggers(float damageAmount)
    {
        for (int i = 0; i < reactionSlots.Count; i++)
        {
            var slot = reactionSlots[i];
            if (slot.cardData == null || slot.isOnCooldown) continue;

            if (slot.cardData.triggerType == ReactionTriggerType.OnDamaged)
            {
                TriggerReaction(i);
            }
        }
    }

    // Check triggers berdasarkan time interval
    void CheckTimeIntervalTriggers()
    {
        for (int i = 0; i < reactionSlots.Count; i++)
        {
            var slot = reactionSlots[i];
            if (slot.cardData == null || slot.isOnCooldown) continue;

            if (slot.cardData.triggerType == ReactionTriggerType.TimeInterval)
            {
                if (timeIntervalTimer >= slot.cardData.triggerThreshold)
                {
                    TriggerReaction(i);
                    timeIntervalTimer = 0;
                }
            }
        }
    }

    // Trigger reaction card effect
    void TriggerReaction(int slotIndex)
    {
        var slot = reactionSlots[slotIndex];
        if (slot.cardData == null) return;

        Debug.Log($"[ReactionDeck] Triggering {slot.cardData.cardName} from slot {slotIndex}");

        // Execute card effect
        ExecuteReactionEffect(slot.cardData);

        // Set cooldown
        slot.isOnCooldown = true;
        slot.cooldownTimer = slot.cardData.cooldown;

        // Trigger event untuk UI
        OnReactionTriggered?.Invoke(slot.cardData, slotIndex);
    }

    // Execute reaction card effect
    void ExecuteReactionEffect(CardData card)
    {
        switch (card.cardType)
        {
            case CardType.Recovery:
                if (playerHealth != null)
                {
                    playerHealth.Heal(card.healAmount);
                    Debug.Log($"[ReactionDeck] Healed {card.healAmount} HP");
                }
                break;

            case CardType.Defense:
                if (playerHealth != null)
                {
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

    IEnumerator ApplyBuff(CardData card)
    {
        Debug.Log($"[ReactionDeck] Applied buff: {card.cardName} for {card.effectDuration}s");
        // Implement buff logic here
        // Example: increase move speed, damage, etc
        yield return new WaitForSeconds(card.effectDuration);
        Debug.Log($"[ReactionDeck] Buff {card.cardName} expired");
    }

    // Helper method untuk add empty slot
    void AddEmptySlot()
    {
        if (reactionSlots.Count < maxReactionSlots)
        {
            reactionSlots.Add(new ReactionCardSlot());
        }
    }

    // Public getters
    public List<ReactionCardSlot> GetReactionSlots() => reactionSlots;
    public int GetAvailableSlots() => maxReactionSlots - reactionSlots.Count;
    public bool HasEmptySlot()
    {
        foreach (var slot in reactionSlots)
            if (slot.cardData == null) return true;
        return false;
    }
}

// Class untuk menyimpan reaction card slot data
[System.Serializable]
public class ReactionCardSlot
{
    public CardData cardData;
    public bool isOnCooldown;
    public float cooldownTimer;
}