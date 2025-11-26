using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New DamageEffect", menuName = "Card Effects/Damage")]
    public class DamageEffect : CardEffect {
        public int damageAmount;
        public float knockbackForce;
        // public DamageType damageType; // (Opsional: Fire, Ice, dll)

        public override void Execute(GameObject user, GameObject target) {
            if (target != null) {
                // Try EnemyStats first (for Knockback)
                var enemyStats = target.GetComponent<EnemyStats>();
                if (enemyStats != null) {
                    enemyStats.TakeDamage(damageAmount, user.transform.position, knockbackForce);
                } else {
                    // Fallback to PlayerStats or generic EntityStats
                    target.GetComponent<EntityStats>()?.TakeDamage(damageAmount);
                }
                
                // Debug.Log($"Menyerang {target.name} sebesar {damageAmount}!");
            }
        }
    }
}