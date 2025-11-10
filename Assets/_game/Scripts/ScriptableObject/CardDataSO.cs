using UnityEngine;

namespace Littale {
    public enum CardType { Melee, Range, Buff, Utility, Recovery, Defense, Active }

    // Enum untuk tipe trigger reaction
    public enum ReactionTriggerType {
        HealthBelow,      // Trigger saat HP < threshold
        HealthAbove,      // Trigger saat HP > threshold
        OnDamaged,        // Trigger saat player terkena damage
        OnKill,           // Trigger saat membunuh enemy
        TimeInterval      // Trigger setiap X detik
    }

    // Enum untuk tipe active skill
    public enum ActiveSkillType {
        AreaDamage,       // Area damage (circle/radius)
        BurstDamage,      // Single target burst
        MultiShot,        // Multiple projectiles
        Buff,             // Temporary buff
        Heal,             // Instant heal
        TimeSlow,         // Slow time effect
        Teleport          // Dash/teleport
    }

    [CreateAssetMenu(fileName = "New Card", menuName = "LittaleData/Card")]
    public class CardDataSO : ScriptableObject {
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

        [Header("Active Card Settings (Ultimate)")]
        public bool isActiveCard = false;

        [Tooltip("Tipe active skill")]
        public ActiveSkillType activeSkillType = ActiveSkillType.AreaDamage;

        [Tooltip("Apakah skill ini channeling/continuous?")]
        public bool isContinuousSkill = false;

        [Tooltip("Durasi skill channeling (0 = instant, >0 = continuous)")]
        public float skillDuration = 0f;

        [Tooltip("Interval spawn untuk continuous skill (detik)")]
        public float spawnInterval = 0.5f;

        [Tooltip("Range/Radius untuk area effect")]
        public float skillRange = 5f;

        [Tooltip("Visual effect prefab untuk active skill")]
        public GameObject skillEffectPrefab;

        [Tooltip("Projectile prefab untuk skill yang butuh projectile")]
        public GameObject skillProjectilePrefab;

        [Tooltip("Number of projectiles/hits untuk multi-hit skills")]
        public int multiHitCount = 1;

        [Tooltip("Mana cost atau energy cost (optional)")]
        public int manaCost = 0;
    }
}