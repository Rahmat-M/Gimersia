using UnityEngine;
using System.Collections.Generic;

namespace Littale {
    public class ProjectileController : MonoBehaviour {
        [Header("Settings")]
        public float speed = 10f;
        public float lifetime = 5f;
        
        // Pierce Logic
        public int pierceCount = 0; // 0 = Destroy on first hit, >0 = Pierce N times, 999 = Infinite

        // Events
        public System.Action OnKill;

        private List<CardEffect> effectsToApply;
        private GameObject owner;
        private Vector3 moveDirection;

        public void Initialize(List<CardEffect> effects, GameObject user, Vector3 direction) {
            effectsToApply = effects;
            owner = user;
            moveDirection = direction.normalized;

            if (moveDirection != Vector3.zero) {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            if (lifetime > 0) Destroy(gameObject, lifetime);
        }
        
        public void SetPierce(int count) {
            pierceCount = count;
        }

        // Arcing Logic
        private bool isArcing = false;
        private Vector3 arcStart;
        private Vector3 arcEnd;
        private float arcHeight = 2f;
        private float arcDuration = 1f;
        private float arcTimer = 0f;
        private System.Action onArcComplete;

        public void LaunchTo(Vector3 target, System.Action onReach) {
            isArcing = true;
            arcStart = transform.position;
            arcEnd = target;
            onArcComplete = onReach;
            arcTimer = 0f;
            speed = 0; // Disable linear movement
        }

        void Update() {
            if (isArcing) {
                arcTimer += Time.deltaTime;
                float t = arcTimer / arcDuration;
                
                if (t >= 1f) {
                    transform.position = arcEnd;
                    isArcing = false;
                    onArcComplete?.Invoke();
                } else {
                    // Parabolic Arc
                    Vector3 currentPos = Vector3.Lerp(arcStart, arcEnd, t);
                    currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                    transform.position = currentPos;
                }
            } else if (speed > 0) {
                transform.position += moveDirection * speed * Time.deltaTime;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                ApplyEffects(other.gameObject);
                
                // Mock Kill Check
                // if (other.GetComponent<EnemyHealth>().IsDead()) OnKill?.Invoke();

                if (pierceCount > 0) {
                    pierceCount--;
                    if (pierceCount > 900) pierceCount = 999; // Keep infinite
                } else {
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