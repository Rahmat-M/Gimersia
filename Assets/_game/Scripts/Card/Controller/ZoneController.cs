using UnityEngine;

namespace Littale {
    public class ZoneController : MonoBehaviour {
        public enum ZoneType { Damage, Heal, Buff, Debuff }
        
        [Header("Settings")]
        public ZoneType type;
        public float duration = 5f;
        public float interval = 1f;
        
        [Header("Values")]
        public float amount = 10f; // Damage or Heal amount
        public bool clearBullets = false; // For Eraser Gold
        public bool applyWet = false; // For Water Bucket
        public bool armorBreak = false; // For Eraser Silver

        private float timer;

        public void Initialize(ZoneType zType, float val, float dur) {
            type = zType;
            amount = val;
            duration = dur;
            Destroy(gameObject, duration);
        }

        void Update() {
            timer += Time.deltaTime;
            if (timer >= interval) {
                ApplyZoneEffect();
                timer = 0f;
            }

            if (clearBullets) {
                // Clear bullets in radius
                Collider2D[] bullets = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2f); // Approx radius
                foreach (var b in bullets) {
                    if (b.CompareTag("EnemyBullet")) Destroy(b.gameObject);
                }
            }
        }

        void ApplyZoneEffect() {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2f);
            foreach (var hit in hits) {
                if (type == ZoneType.Heal && hit.CompareTag("Player")) {
                    hit.GetComponent<PlayerStats>()?.RestoreHealth(amount);
                }
                else if (type == ZoneType.Damage && hit.CompareTag("Enemy")) {
                    // hit.GetComponent<EnemyHealth>()?.TakeDamage(amount);
                    if (armorBreak) {
                        // hit.GetComponent<EnemyStats>()?.ApplyArmorBreak();
                    }
                    if (applyWet) {
                        // hit.GetComponent<EnemyStats>()?.ApplyStatus("Wet");
                    }
                }
            }
        }
    }
}
