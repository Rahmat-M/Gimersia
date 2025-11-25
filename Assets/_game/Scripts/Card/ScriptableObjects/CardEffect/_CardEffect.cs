using UnityEngine;

namespace Littale {
    public abstract class CardEffect : ScriptableObject {
        public abstract void Execute(GameObject user, GameObject target);
    }
}