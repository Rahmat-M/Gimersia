using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Reactive Card", menuName = "Littale Data/Card/Reactive")]
    public class ReactiveCardSO : BaseCardSO {

        public enum CardType { Buff, Utility, Recovery, Defense }
        public enum ReactiveType {
            HealthBelow,      // Trigger saat HP < threshold
            HealthAbove,      // Trigger saat HP > threshold
            OnDamaged,        // Trigger saat player terkena damage
            OnKill,           // Trigger saat membunuh enemy
            TimeInterval      // Trigger setiap X detik
        }

        [Header("Stats")]
        [SerializeField] CardType type;
        public CardType Type { get { return type; } }

        [SerializeField] ReactiveType reactiveType;
        public ReactiveType ReactionType { get { return reactiveType; } }

        [SerializeField] bool noCooldown;
        public bool NoCooldown { get { return noCooldown; } }

        [SerializeField] float cooldown;
        public float Cooldown { get { return cooldown; } }

        [SerializeField] float triggerThreshold;
        public float TriggerThreshold { get { return triggerThreshold; } }

        [SerializeField] int effectAmount;
        public int EffectAmount { get { return effectAmount; } }

        [SerializeField] float effectDuration;
        public float EffectDuration { get { return effectDuration; } }

    }
}