using UnityEngine;

namespace Littale {
    /// <summary>
    /// A class that represents a passive card that applies modifiers to the player.
    /// </summary>
    public abstract class PassiveCardController : BaseCard {

        [SerializeField] CharacterSO.Stats currentBoosts;

        public virtual void Init(PassiveCardSO data) {
            currentBoosts = data.baseStats.boosts;
        }

        public virtual CharacterSO.Stats GetBoosts() {
            return currentBoosts;
        }

    }
}