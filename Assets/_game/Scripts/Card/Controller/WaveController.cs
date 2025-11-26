using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class WaveController : MonoBehaviour {
        [Header("Settings")]
        public float speed = 15f;
        public float lifetime = 1f;
        public float pushForce = 10f;
        public bool canStun = false;

        private List<CardEffect> effectsToApply;
        private GameObject owner;
        private Vector3 moveDirection;

        public void Initialize(float force, bool stun, GameObject user) {
            pushForce = force;
            canStun = stun;
            owner = user;
            moveDirection = transform.right; // Moves forward relative to rotation
            Destroy(gameObject, lifetime);
        }

        void Update() {
            transform.position += moveDirection * speed * Time.deltaTime;
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                // Apply Pushback
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb) {
                    Vector2 pushDir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(pushDir * pushForce, ForceMode2D.Impulse);
                }

                // Apply Stun if enabled
                if (canStun) {
                    // Assuming EnemyBehavior has a Stun method
                    // other.GetComponent<EnemyBehavior>()?.Stun(1f);
                }
            }
        }
    }
}
