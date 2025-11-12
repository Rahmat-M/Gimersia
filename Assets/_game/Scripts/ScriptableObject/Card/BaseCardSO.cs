using UnityEngine;

namespace Littale {
    public enum CardType {
        MainDeck,
        Passive,
        Reactive,
        Active
    }

    public abstract class BaseCardSO : ScriptableObject {

        [Header("Basic Info")]
        [SerializeField] GameObject prefab;
        public GameObject Prefab { get { return prefab; } }

        public CardType cardType;
        public Sprite icon;
        public string cardName;
        [TextArea] public string description;

    }
}