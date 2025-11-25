using UnityEngine;

namespace Littale {
    public class UniversalCardController : CardController {

        public override void Attack() {
            // if (cardData == null) {
            //     Debug.LogWarning($"CardController on {gameObject.name} has no CardData!");
            //     return;
            // }

            // if (cardData.Actions == null || cardData.Actions.Count == 0) {
            //     // Fallback for legacy cards or empty actions
            //     // We could log a warning, but for now let's just return.
            //     // Debug.LogWarning($"Card {cardData.name} has no Actions assigned!");
            //     return;
            // }

            // foreach (var action in cardData.Actions) {
            //     action.PerformAction(this, cardData);
            // }
        }

    }
}
