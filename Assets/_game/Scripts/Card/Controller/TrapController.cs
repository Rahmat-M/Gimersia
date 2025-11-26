using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class TrapController : MonoBehaviour {
        [Header("Settings")]
        public float duration = 4f;
        public float slowAmount = 0.4f; // 0.4 = 40% slow
        public bool isCorrosive = false;
        public float damageInterval = 0.5f;
        public float damagePerTick = 5f;

        private float tickTimer;

        public void Initialize(float slow, bool corrosive) {
            slowAmount = slow;
            isCorrosive = corrosive;
            Destroy(gameObject, duration);
        }

        void OnTriggerStay2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                // Apply Slow
                // Assuming EnemyMovement script handles speed
                // other.GetComponent<EnemyMovement>()?.ApplySlow(slowAmount);

                // Apply Corrosive Damage
                if (isCorrosive) {
                    tickTimer += Time.deltaTime;
                    if (tickTimer >= damageInterval) {
                        // other.GetComponent<EnemyHealth>()?.TakeDamage(damagePerTick);
                        tickTimer = 0f;
                    }
                }
            }
        }

        void OnTriggerExit2D(Collider2D other) {
             if (other.CompareTag("Enemy")) {
                // Remove Slow
                // other.GetComponent<EnemyMovement>()?.RemoveSlow();
             }
        }
    }
}
