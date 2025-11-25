using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class ProjectileController : MonoBehaviour {
        [Header("Settings")]
        public float speed = 10f;      // Kecepatan gerak (0 = diam/melee)
        public float lifetime = 5f;    // Waktu hidup sebelum hancur otomatis (0 = selamanya)
        public bool penetrate = false; // Tembus musuh atau tidak

        private List<CardEffect> effectsToApply;
        private GameObject owner;

        private Vector3 moveDirection;

        public void Initialize(List<CardEffect> effects, GameObject user, Vector3 direction) {
            effectsToApply = effects;
            owner = user;
            moveDirection = direction.normalized;

            // Putar proyektil sesuai arah gerak
            if (moveDirection != Vector3.zero) {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            if (lifetime > 0) Destroy(gameObject, lifetime);
        }

        void Update() {
            if (speed > 0) {
                transform.position += moveDirection * speed * Time.deltaTime;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                ApplyEffects(other.gameObject);

                if (!penetrate) {
                    Destroy(gameObject);
                }
            }
        }

        void ApplyEffects(GameObject target) {
            if (effectsToApply == null) return;

            foreach (var effect in effectsToApply) {
                effect.Execute(owner, target);
            }
        }
    }
}