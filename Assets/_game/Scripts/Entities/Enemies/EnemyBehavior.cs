using UnityEngine;

namespace Littale {
    public abstract class EnemyBehavior : MonoBehaviour {
        protected EnemyStats stats;
        protected SpriteRenderer spriteRenderer;

        protected virtual void Awake() {
            stats = GetComponent<EnemyStats>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected virtual void Start() {
            // Optional: Hook into stats events if needed
        }

        protected virtual void Update() {
            // Common behavior loop
        }

        public virtual void OnDeath() {
            // Override for death effects
        }
    }
}
