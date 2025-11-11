using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Passive Card", menuName = "Littale Data/Card/Passive")]
    public class PassiveCardSO : BaseCardSO {

        [System.Serializable]
        public class Modifier {
            public CharacterSO.Stats boosts;
        }

        public Modifier baseStats;

    }
}