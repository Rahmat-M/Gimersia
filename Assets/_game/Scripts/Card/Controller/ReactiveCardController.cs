using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public abstract class ReactiveCardController : BaseCard {

        public UnityAction<int> OnCardPutOnCooldown;
        public UnityAction<float, int> OnCooldownTick;
        public UnityAction OnCardReady;

        [Header("Card Stats")]
        public ReactiveCardSO cardData;

        bool onCooldown; // Is the card on cooldown
        float cooldownTimer;

        bool isTimeInterval; // Flag to check if the card uses TimeInterval reaction
        float currentInterval;

        protected override void Start() {
            base.Start();
            CheckReactionType();
        }

        void Update() {
            if (onCooldown) {
                cooldownTimer -= Time.deltaTime;
                float progress = 1f - (cooldownTimer / cardData.Cooldown);
                OnCooldownTick?.Invoke(progress, Mathf.CeilToInt(cooldownTimer));

                if (cooldownTimer <= 0f) {
                    onCooldown = false;
                    OnCardReady?.Invoke();
                    CheckConditionsAfterCooldown();
                }
            }

            if (!onCooldown) {
                CheckIntervalTime();
            }
        }

        // Implement the specific reaction effect in derived classes
        protected virtual void Apply() { }

        protected virtual void Applied() {
            if (cardData.NoCooldown) return;
            cooldownTimer = cardData.Cooldown;
            OnCardPutOnCooldown?.Invoke(Mathf.CeilToInt(cardData.Cooldown));
            onCooldown = true;
        }

        public void Trigger() {
            if (onCooldown) {
                Debug.LogWarning("Card is on cooldown!");
                return;
            }

            Apply();
            Applied();
        }

        void CheckReactionType() {
            switch (cardData.ReactionType) {
                case ReactiveCardSO.ReactiveType.HealthBelow:
                    characterStats.OnHealthChanged += CheckHealthBelow;
                    break;
                case ReactiveCardSO.ReactiveType.HealthAbove:
                    characterStats.OnHealthChanged += CheckHealthAbove;
                    break;
                case ReactiveCardSO.ReactiveType.OnDamaged:
                    characterStats.OnHealthChanged += CheckGotDamaged;
                    break;
                case ReactiveCardSO.ReactiveType.OnKill:
                    characterStats.OnKilled += CheckGotKilled;
                    break;
                case ReactiveCardSO.ReactiveType.TimeInterval:
                    currentInterval = cardData.TriggerThreshold;
                    isTimeInterval = true;
                    break;
                default:
                    Debug.LogError("Reaction type not handled: " + cardData.ReactionType);
                    break;
            }
        }

        void CheckConditionsAfterCooldown() {
            if (onCooldown) return;
            if (characterStats == null) return;

            switch (cardData.ReactionType) {
                case ReactiveCardSO.ReactiveType.HealthBelow:
                    CheckHealthBelow(characterStats.CurrentHealth);
                    break;
                case ReactiveCardSO.ReactiveType.HealthAbove:
                    CheckHealthAbove(characterStats.CurrentHealth);
                    break;
                case ReactiveCardSO.ReactiveType.OnDamaged:
                case ReactiveCardSO.ReactiveType.OnKill:
                case ReactiveCardSO.ReactiveType.TimeInterval:
                default:
                    break;
            }
        }

        void CheckHealthBelow(float currentHealth) {
            if (currentHealth <= cardData.TriggerThreshold) {
                Trigger();
            }
        }

        void CheckHealthAbove(float currentHealth) {
            if (currentHealth >= cardData.TriggerThreshold) {
                Trigger();
            }
        }

        void CheckGotDamaged(float _) => Trigger();
        void CheckGotKilled() => Trigger();

        void CheckIntervalTime() {
            if (isTimeInterval) {
                currentInterval -= Time.deltaTime;
                if (currentInterval <= 0f) {
                    Trigger();
                    currentInterval = cardData.TriggerThreshold;
                }
            }
        }

        void OnDestroy() {
            // Unsubscribe from events to prevent memory leaks
            if (characterStats != null) {
                switch (cardData.ReactionType) {
                    case ReactiveCardSO.ReactiveType.HealthBelow:
                        characterStats.OnHealthChanged -= CheckHealthBelow;
                        break;
                    case ReactiveCardSO.ReactiveType.HealthAbove:
                        characterStats.OnHealthChanged -= CheckHealthAbove;
                        break;
                    case ReactiveCardSO.ReactiveType.OnDamaged:
                        characterStats.OnHealthChanged -= CheckGotDamaged;
                        break;
                    case ReactiveCardSO.ReactiveType.OnKill:
                        characterStats.OnKilled -= CheckGotKilled;
                        break;
                }
            }

            // Clear delegates
            OnCardPutOnCooldown = null;
            OnCooldownTick = null;
            OnCardReady = null;
        }

    }
}