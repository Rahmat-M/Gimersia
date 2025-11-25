namespace Littale {
    public class ShieldReactiveCard : ReactiveCardController {

        protected override void Apply() {
            float shieldAmount = cardData.EffectAmount;
            characterStats.CurrentArmor += shieldAmount;
        }

    }
}
