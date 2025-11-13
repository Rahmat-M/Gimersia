using UnityEngine;

namespace Littale {
    /// <summary>
    /// Base script for all main card controllers
    /// </summary>
    public abstract class BaseCard : MonoBehaviour {
        protected CharacterStats characterStats;
        protected CharacterMovement characterMovement;

        protected virtual void Start() {
            characterStats = FindFirstObjectByType<CharacterStats>();
            characterMovement = characterStats.GetComponent<CharacterMovement>();
        }
    }
}