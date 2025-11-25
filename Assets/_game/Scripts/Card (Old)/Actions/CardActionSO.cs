using UnityEngine;

namespace Littale {
    public abstract class CardActionSO : ScriptableObject {
        public abstract void PerformAction(BaseCard controller, BaseCardSO data);
    }
}
