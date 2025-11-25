using UnityEngine;

namespace Littale {
    public class HealReactiveCard : ReactiveCardController {
        protected override void Apply() {
            float healAmount = cardData.EffectAmount;
            characterStats.RestoreHealth(healAmount);
        }
    }
}
