using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class MeleeAttackController : MonoBehaviour {
        [Header("Components")]
        public Animator animator;
        public AnimationClip attackClip;

        [Header("Settings")]
        public float activeDuration = 0.2f; // Berapa lama hitbox aktif

        private int damage;
        private bool isDoubleStroke;
        private bool isSiphon;
        private PlayerCardManager playerManager;
        private List<GameObject> hitEnemies = new List<GameObject>();

        public void Initialize(int damage, bool doubleStroke, bool siphon, PlayerCardManager manager) {
            this.damage = damage;
            this.isDoubleStroke = doubleStroke;
            this.isSiphon = siphon;
            this.playerManager = manager;

            if (animator != null && attackClip != null) {
                float originalDuration = attackClip.length;
                float targetSpeed = originalDuration / activeDuration;

                animator.speed = targetSpeed;
            } else {
                Debug.LogWarning("Animator atau Clip belum di-assign di MeleeAttackController!");
            }

            Destroy(gameObject, activeDuration); // Hapus otomatis
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy") && !hitEnemies.Contains(other.gameObject)) {
                hitEnemies.Add(other.gameObject);

                var enemyStats = other.GetComponent<EnemyStats>();
                if (enemyStats != null) {
                    bool isDead = enemyStats.TakeDamage(damage);

                    if (isSiphon && isDead) {
                        playerManager.RegenMana(1);
                    }
                }
            }
        }
    }
}