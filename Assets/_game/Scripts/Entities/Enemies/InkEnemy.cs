using UnityEngine;

namespace Littale {
    public class InkEnemy : EnemyBehavior {
        public enum InkType { Blob, Kamikaze, Golem }
        public InkType type;

        [Header("Blob Settings")]
        public GameObject puddlePrefab;

        [Header("Kamikaze Settings")]
        public float explodeRadius = 3f;
        public int explodeDamage = 20;
        public GameObject explosionEffect;

        protected override void Start() {
            base.Start();
            InitializeVisuals();
        }

        void InitializeVisuals() {
            if (spriteRenderer == null) return;
            spriteRenderer.color = Color.black;

            switch (type) {
                case InkType.Blob:
                    transform.localScale = Vector3.one;
                    break;
                case InkType.Kamikaze:
                    transform.localScale = Vector3.one * 0.8f;
                    // Maybe pulse effect in Update
                    break;
                case InkType.Golem:
                    transform.localScale = Vector3.one * 2.0f;
                    break;
            }
        }

        protected override void Update() {
            base.Update();
            if (type == InkType.Kamikaze) {
                CheckExplosion();
            }
        }

        void CheckExplosion() {
            PlayerStats player = FindFirstObjectByType<PlayerStats>();
            if (player && Vector3.Distance(transform.position, player.transform.position) < 1.5f) {
                Explode();
            }
        }

        void Explode() {
            // Deal damage to player
            PlayerStats player = FindFirstObjectByType<PlayerStats>();
            if (player && Vector3.Distance(transform.position, player.transform.position) < explodeRadius) {
                player.TakeDamage(explodeDamage);
            }
            
            if (explosionEffect) Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        public override void OnDeath() {
            if (type == InkType.Blob && puddlePrefab != null) {
                Instantiate(puddlePrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
