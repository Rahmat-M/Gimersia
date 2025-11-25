using UnityEngine;

namespace Littale {
    /// <summary>
    /// Base script for all main card controllers
    /// </summary>
    public abstract class CardController : BaseCard {

        [Header("Card Stats")]
        public MainCardSO cardData;

        public virtual void Attack() { }

    }
}