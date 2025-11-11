using UnityEngine;

namespace Littale {
    public class ShieldReactiveCard : ReactiveCardController {

        protected override void Apply() {
            float shieldAmount = cardData.EffectAmount;
            characterStats.Stats += new CharacterSO.Stats { armor = shieldAmount };
        }

    }
}
