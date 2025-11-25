using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New DamageEffect", menuName = "Card Effects/Damage")]
    public class DamageEffect : CardEffect {
        public int damageAmount;
        // public DamageType damageType; // (Opsional: Fire, Ice, dll)

        public override void Execute(GameObject user, GameObject target) {
            if (target != null) {
                target.GetComponent<PlayerStats>()?.TakeDamage(damageAmount);
                Debug.Log($"Menyerang {target.name} sebesar {damageAmount}!");
            }
        }
    }
}