namespace Littale {
    public class HealActiveCard : ActiveCardController {

        protected override void Start() {
            base.Start();
            characterMovement.TriggerActions.action.performed += ctx => Trigger();
        }

        protected override void Activate() {
            float healAmount = cardData.EffectAmount;
            characterStats.RestoreHealth(healAmount);
        }

        void OnDestroy() {
            if (characterMovement != null) {
                characterMovement.TriggerActions.action.performed -= ctx => Trigger();
            }
        }

    }
}