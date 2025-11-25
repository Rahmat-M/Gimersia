using UnityEngine;

namespace Littale {
    public abstract class BaseCardSO : ScriptableObject {

        [Header("Basic Info")]
        [SerializeField] GameObject prefab;
        public GameObject Prefab { get { return prefab; } }

        [SerializeField] GameObject behaviourPrefab;
        public GameObject BehaviourPrefab { get { return behaviourPrefab; } }

        public CardType cardType;
        public Sprite icon;
        public string cardName;
        [TextArea] public string description;

        public string GetCardTypeString() {
            switch (cardType) {
                // case CardType.Main:
                //     return "Main Deck";
                // case CardType.Passive:
                //     return "Passive";
                // case CardType.Reactive:
                //     return "Reactive";
                // case CardType.Active:
                //     return "Active";
                default:
                    return "Unknown";
            }
        }

    }
}