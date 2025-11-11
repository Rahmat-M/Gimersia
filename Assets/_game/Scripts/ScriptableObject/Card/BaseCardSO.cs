using UnityEngine;

namespace Littale {
    public abstract class BaseCardSO : ScriptableObject {

        [Header("Basic Info")]
        [SerializeField] GameObject prefab;
        public GameObject Prefab { get { return prefab; } }

        public Sprite icon;
        public string cardName;
        [TextArea] public string description;

    }
}