using UnityEngine;

public enum CardType { Melee, Range, Buff, Utility, Recovery, Defense }

// Enum untuk tipe trigger reaction
public enum ReactionTriggerType
{
    HealthBelow,      // Trigger saat HP < threshold
    HealthAbove,      // Trigger saat HP > threshold
    OnDamaged,        // Trigger saat player terkena damage
    OnKill,           // Trigger saat membunuh enemy
    TimeInterval      // Trigger setiap X detik
}

[CreateAssetMenu(fileName = "Card_", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public CardType cardType;
    public int damage;
    public float cooldown; // cooldown untuk reaction atau attack
    public Sprite icon;
    [TextArea] public string description;
    public int amount = 1; // jumlah serangan atau heal amount

    [Header("Reaction Card Settings")]
    public bool isReactionCard = false;

    [Tooltip("Tipe trigger untuk reaction card")]
    public ReactionTriggerType triggerType = ReactionTriggerType.HealthBelow;

    [Tooltip("Threshold value (0-100 untuk health percentage, atau value lain sesuai trigger type)")]
    [Range(0, 100)]
    public float triggerThreshold = 75f;

    [Tooltip("Healing amount untuk Recovery type")]
    public int healAmount = 0;

    [Tooltip("Shield/Defense amount untuk Defense type")]
    public int defenseAmount = 0;

    [Tooltip("Duration untuk buff/shield (dalam detik)")]
    public float effectDuration = 5f;
}