using UnityEngine;

namespace Littale {
    /// <summary>
    /// Base script for all main card controllers
    /// </summary>
    public abstract class BaseCard : MonoBehaviour {
        protected PlayerStats characterStats;
        protected PlayerMovement characterMovement;

        protected virtual void Start() {
            characterStats = FindFirstObjectByType<PlayerStats>();
            characterMovement = characterStats.GetComponent<PlayerMovement>();
        }

        public Vector2 GetMovementInput() {
            return characterMovement != null ? characterMovement.LastMovementInput : Vector2.zero;
        }
    }
}