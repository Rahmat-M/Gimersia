using UnityEngine;
using System.Collections;

namespace Littale {
    public class PaperEnemy : EnemyBehavior {
        public enum PaperType { Origami, Plane, Guard }
        public PaperType type;

        [Header("Plane Settings")]
        public float dashSpeed = 15f;
        public float dashDuration = 0.5f;
        public float chargeTime = 1.0f;
        private bool isDashing;

        [Header("Guard Settings")]
        public float shieldReduction = 0.8f; // 80% damage reduction from front

        protected override void Start() {
            base.Start();
            InitializeVisuals();
            if (type == PaperType.Plane) StartCoroutine(PlaneRoutine());
        }

        void InitializeVisuals() {
            if (spriteRenderer == null) return;

            switch (type) {
                case PaperType.Origami:
                    spriteRenderer.color = Color.white;
                    transform.localScale = Vector3.one * 0.8f;
                    break;
                case PaperType.Plane:
                    spriteRenderer.color = new Color(0.9f, 0.9f, 1f); // Bluish white
                    transform.localScale = new Vector3(1.2f, 0.6f, 1f);
                    break;
                case PaperType.Guard:
                    spriteRenderer.color = new Color(0.6f, 0.5f, 0.4f); // Cardboard brown
                    transform.localScale = Vector3.one * 1.5f;
                    break;
            }
        }

        IEnumerator PlaneRoutine() {
            while (true) {
                yield return new WaitForSeconds(2f); // Wait between attacks
                
                // Charge
                spriteRenderer.color = Color.red; // Warning flash
                yield return new WaitForSeconds(chargeTime);
                
                // Dash
                isDashing = true;
                Vector3 dashDir = (FindFirstObjectByType<PlayerStats>().transform.position - transform.position).normalized;
                float timer = 0;
                spriteRenderer.color = Color.white;
                
                while (timer < dashDuration) {
                    transform.position += dashDir * dashSpeed * Time.deltaTime;
                    timer += Time.deltaTime;
                    yield return null;
                }
                isDashing = false;
            }
        }

        // Hook this into EnemyStats TakeDamage if possible, or handle collision logic here
        public float ModifyDamage(float damage, Vector3 sourcePos) {
            if (type == PaperType.Guard) {
                // Check if attack is from front
                Vector3 dirToSource = (sourcePos - transform.position).normalized;
                float dot = Vector3.Dot(transform.right, dirToSource); // Assuming right is forward for 2D sprite facing
                
                // Adjust dot check based on actual sprite orientation
                if (dot > 0) { // Hit from front
                    return damage * (1 - shieldReduction);
                }
            }
            return damage;
        }
    }
}
