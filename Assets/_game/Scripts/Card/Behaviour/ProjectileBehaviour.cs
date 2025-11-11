using UnityEngine;

namespace Littale {
    /// <summary>
    /// Base script of all projectile behaviours [To be placed on a prefab of a weapon that is a projectile]
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileBehaviour : MonoBehaviour {

        public MainCardSO weaponData;

        protected Vector3 direction;

        public float destroyAfterSeconds;

        //Current stats
        protected float currentDamage;
        protected float currentSpeed;
        protected float currentCooldownDuration;
        protected int currentPierce;

        void Awake() {
            currentDamage = weaponData.Damage;
            currentSpeed = weaponData.Speed;
            currentPierce = weaponData.Pierce;
        }

        public float GetCurrentDamage() {
            return currentDamage *= FindFirstObjectByType<CharacterStats>().Actual.might;
        }

        protected virtual void Start() {
            Destroy(gameObject, destroyAfterSeconds);
        }

        public void DirectionChecker(Vector3 dir) {
            direction = dir;

            float dirx = direction.x;
            float diry = direction.y;

            float angleRadians = Mathf.Atan2(diry, dirx);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            Vector3 rotation = new Vector3(0, 0, angleDegrees);

            transform.rotation = Quaternion.Euler(rotation);    //Can't simply set the vector because cannot convert
        }

        protected virtual void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.CompareTag("Enemy")) {
                if (col.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyStats)) {
                    enemyStats.TakeDamage(GetCurrentDamage());
                    ReducePierce();
                }
            }
        }

        void ReducePierce() { //Destroy once the pierce reaches 0
            currentPierce--;
            if (currentPierce <= 0) {
                Destroy(gameObject);
            }
        }

    }
}