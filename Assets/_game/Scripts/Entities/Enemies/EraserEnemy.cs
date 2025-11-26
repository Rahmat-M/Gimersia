using UnityEngine;

namespace Littale {
    public class EraserEnemy : EnemyBehavior {
        public enum EraserType { Cube, Dust }
        public EraserType type;

        [Header("Cube Settings")]
        public float deleteRadius = 2f;

        [Header("Dust Settings")]
        public float auraRadius = 5f;
        public GameObject dustCloudVisual;

        protected override void Start() {
            base.Start();
            InitializeVisuals();
        }

        void InitializeVisuals() {
            if (spriteRenderer == null) return;
            spriteRenderer.color = Color.white; // Eraser white

            switch (type) {
                case EraserType.Cube:
                    transform.localScale = Vector3.one;
                    break;
                case EraserType.Dust:
                    transform.localScale = Vector3.one * 1.2f;
                    if (dustCloudVisual) dustCloudVisual.SetActive(true);
                    break;
            }
        }

        protected override void Update() {
            base.Update();
            if (type == EraserType.Cube) {
                DeleteProjectiles();
            } else if (type == EraserType.Dust) {
                ApplyAura();
            }
        }

        void DeleteProjectiles() {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, deleteRadius);
            foreach (var hit in hits) {
                if (hit.GetComponent<ProjectileController>()) {
                    Destroy(hit.gameObject);
                    // Optional: Play "Erased" effect
                }
            }
        }

        void ApplyAura() {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, auraRadius);
            foreach (var hit in hits) {
                EnemyStats es = hit.GetComponent<EnemyStats>();
                if (es != null && es.gameObject != gameObject) {
                    // Apply Invincibility or High Defense buff
                    // For now, let's assume we can set a flag or buff
                    // es.SetInvincible(true); // Need to implement this in EnemyStats
                }
            }
        }
    }
}
