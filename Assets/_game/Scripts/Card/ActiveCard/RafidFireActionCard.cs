using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class RafidFireActiveCard : ActiveCardController {

        public float fireRate = 0.2f;

        private Coroutine shootingCoroutine;
        private bool isShooting = false;

        protected override void Start() {
            base.Start();
            characterMovement.TriggerActions.action.performed += HandleInput;
        }

        private void HandleInput(InputAction.CallbackContext ctx) {
            Trigger();
        }

        protected override void Activate() {
            if (isShooting) return;

            shootingCoroutine = StartCoroutine(FireBarrageRoutine());
        }

        IEnumerator FireBarrageRoutine() {
            isShooting = true;
            float endTime = Time.time + cardData.EffectDuration;

            while (Time.time < endTime) {
                FireOneProjectile();

                // wait for next shot (fire rate)
                yield return new WaitForSeconds(fireRate);
            }

            isShooting = false;
            shootingCoroutine = null;
        }

        void FireOneProjectile() {
            if (cardData.SkillProjectilePrefab == null) return;

            Vector2 aimDirection = characterMovement.LastMovementInput.normalized;

            if (aimDirection == Vector2.zero) aimDirection = Vector2.right;

            float randomSpread = Random.Range(-5f, 5f);
            Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);
            Vector2 finalDirection = spreadRotation * aimDirection;

            GameObject projObj = Instantiate(cardData.SkillProjectilePrefab, transform.position, Quaternion.identity);
            SoundManager.Instance.Play("projectile_splash");

            if (projObj.TryGetComponent(out ProjectileBehaviour projScript)) {
                projScript.DirectionChecker(finalDirection);
            }
        }

        void OnDestroy() {
            if (characterMovement != null) {
                characterMovement.TriggerActions.action.performed -= HandleInput;
            }

            if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
        }

    }
}