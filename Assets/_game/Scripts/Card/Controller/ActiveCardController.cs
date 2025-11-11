using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public abstract class ActiveCardController : BaseCard {

        public UnityAction<int> OnCardPutOnCooldown;
        public UnityAction<float, int> OnCooldownTick;
        public UnityAction OnCardReady;

        [Header("Card Stats")]
        public ActiveCardSO cardData;

        bool onCooldown; // Is the card on cooldown
        float cooldownTimer;

        void Update() {
            if (onCooldown) {
                cooldownTimer -= Time.deltaTime;
                float progress = 1f - (cooldownTimer / cardData.Cooldown);
                OnCooldownTick?.Invoke(progress, Mathf.CeilToInt(cooldownTimer));

                if (cooldownTimer <= 0f) {
                    onCooldown = false;
                    OnCardReady?.Invoke();
                }
            }
        }

        public void Trigger() {
            if (onCooldown) {
                Debug.LogWarning("Card is on cooldown!");
                return;
            }

            Activate();
            Activated();
        }

        // Implement the specific reaction effect in derived classes
        protected virtual void Activate() { }

        protected virtual void Activated() {
            cooldownTimer = cardData.Cooldown;
            OnCardPutOnCooldown?.Invoke(Mathf.CeilToInt(cardData.Cooldown));
            onCooldown = true;
        }

    }
}