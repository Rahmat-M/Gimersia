using UnityEngine;

namespace Littale {
    /// <summary>
    /// A class that represents a passive card that applies modifiers to the player.
    /// </summary>
    public class PassiveCardController : BaseCard {

        public PassiveCardSO cardData;

        public virtual CharacterSO.Stats GetBoosts() {
            return cardData.baseStats.boosts;
        }

    }
}