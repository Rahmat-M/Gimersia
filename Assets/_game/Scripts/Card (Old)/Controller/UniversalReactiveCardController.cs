using UnityEngine;

namespace Littale {
    public class UniversalReactiveCardController : ReactiveCardController {

        protected override void Apply() {
            if (cardData == null) {
                Debug.LogWarning($"ReactiveCardController on {gameObject.name} has no CardData!");
                return;
            }

            if (cardData.Actions == null || cardData.Actions.Count == 0) {
                return;
            }

            foreach (var action in cardData.Actions) {
                action.PerformAction(this, cardData);
            }
        }

    }
}
