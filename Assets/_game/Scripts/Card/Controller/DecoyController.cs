using UnityEngine;

namespace Littale {
    public class DecoyController : MonoBehaviour {
        [Header("Settings")]
        public float maxHealth = 50f;
        public float duration = 10f;
        public bool explodeOnDeath = false;
        public float explosionDamage = 50f;
        public float explosionRadius = 3f;
        public GameObject explosionPrefab;

        private float currentHealth;

        public void Initialize(float hp, bool explode) {
            maxHealth = hp;
            currentHealth = maxHealth;
            explodeOnDeath = explode;
            Destroy(gameObject, duration);
        }

        public void TakeDamage(float amount) {
            currentHealth -= amount;
            if (currentHealth <= 0) {
                Die();
            }
        }

        void Die() {
            if (explodeOnDeath) {
                // Explode Logic
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
                foreach (var hit in hits) {
                    if (hit.CompareTag("Enemy")) {
                        // hit.GetComponent<EnemyHealth>()?.TakeDamage(explosionDamage);
                    }
                }
                if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
