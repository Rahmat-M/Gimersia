using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class MeleeAttackController : MonoBehaviour {
        [Header("Components")]
        public Animator animator;
        public AnimationClip attackClip;

        [Header("Settings")]
        public float activeDuration = 0.2f;

        private bool isSiphon;
        private PlayerCardManager playerManager;
        private List<GameObject> hitEnemies = new List<GameObject>();
        private List<CardEffect> effectsToApply;

        public void Initialize(List<CardEffect> effects, bool siphon, PlayerCardManager manager) {
            this.effectsToApply = effects;
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

                // Apply Effects (Damage, Knockback, etc.)
                if (effectsToApply != null) {
                    foreach (var effect in effectsToApply) {
                        effect.Execute(playerManager.gameObject, other.gameObject);
                    }
                }

                var enemyStats = other.GetComponent<EnemyStats>();
                if (enemyStats != null) {
                    bool isDead = enemyStats.Health <= 0; // Check health directly
                    if (isSiphon && isDead) {
                        playerManager.RegenMana(1);
                    }
                }
            }
        }

        void PlayAttackSound() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySFX(SfxID.MeleeHit);
            }
        }
    }
}