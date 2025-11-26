using UnityEngine;
using System.Collections;

namespace Littale {
    public class BossController : EnemyBehavior {
        public enum BossPhase { Sketch, Blot, Erasure }
        public BossPhase currentPhase = BossPhase.Sketch;

        [Header("Boss Stats")]
        public float maxHp = 5000;
        public float currentHp;

        [Header("Phase Settings")]
        public GameObject origamiPrefab;
        public GameObject puddlePrefab;
        public GameObject eraserPrefab;

        protected override void Start() {
            base.Start();
            currentHp = maxHp;
            StartCoroutine(BossRoutine());
        }

        IEnumerator BossRoutine() {
            while (currentHp > 0) {
                switch (currentPhase) {
                    case BossPhase.Sketch:
                        yield return StartCoroutine(PhaseSketch());
                        break;
                    case BossPhase.Blot:
                        yield return StartCoroutine(PhaseBlot());
                        break;
                    case BossPhase.Erasure:
                        yield return StartCoroutine(PhaseErasure());
                        break;
                }
                yield return null;
            }
        }

        IEnumerator PhaseSketch() {
            // Spawn Origami
            for (int i = 0; i < 5; i++) {
                Instantiate(origamiPrefab, transform.position + Random.insideUnitSphere * 3, Quaternion.identity);
                yield return new WaitForSeconds(0.5f);
            }
            
            // Dash Attack
            Vector3 target = FindFirstObjectByType<PlayerStats>().transform.position;
            Vector3 startPos = transform.position;
            float t = 0;
            while (t < 1f) {
                transform.position = Vector3.Lerp(startPos, target, t);
                t += Time.deltaTime * 2f;
                yield return null;
            }
            
            yield return new WaitForSeconds(2f);
        }

        IEnumerator PhaseBlot() {
            // Spawn Puddles
            Instantiate(puddlePrefab, FindFirstObjectByType<PlayerStats>().transform.position, Quaternion.identity);
            yield return new WaitForSeconds(1f);
            
            // Shoot Ink
            // Implement shooting logic here
            
            yield return new WaitForSeconds(2f);
        }

        IEnumerator PhaseErasure() {
            // Summon Eraser
            Instantiate(eraserPrefab, transform.position + Vector3.right * 5, Quaternion.identity);
            
            // Giant Sweep
            // Move across screen
            
            yield return new WaitForSeconds(3f);
        }

        public void TakeDamage(float amount) {
            currentHp -= amount;
            CheckPhaseTransition();
            if (currentHp <= 0) Die();
        }

        void CheckPhaseTransition() {
            float hpPercent = currentHp / maxHp;
            if (hpPercent < 0.3f) currentPhase = BossPhase.Erasure;
            else if (hpPercent < 0.7f) currentPhase = BossPhase.Blot;
        }

        void Die() {
            // Explosion effect
            // Win Game
            Destroy(gameObject);
        }
    }
}
