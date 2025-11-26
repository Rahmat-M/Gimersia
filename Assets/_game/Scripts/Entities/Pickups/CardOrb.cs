using UnityEngine;

namespace Littale {
    public class CardOrb : Pickup {
        [Header("Card Reward")]
        public CardData cardReward;

        public override bool Collect(PlayerStats player, float speed, float lifespan = 0) {
            return base.Collect(player, speed);
            // Logic handled in Update/OnTrigger of Pickup base, but we need to override the actual effect
        }

        protected override void OnDestroy() {
            base.OnDestroy(); // Apply base effects (XP/Health) if any
            
            // Only trigger if we were actually collected (target is set)
            if (target) {
                DeckManager deck = FindFirstObjectByType<DeckManager>();
                if (deck && cardReward) {
                    deck.AddCard(cardReward);
                    Debug.Log($"Picked up Card Orb: {cardReward.cardName}");
                }
            }
        }
    }
}
