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

            Vector3 scale = transform.localScale;
            Vector3 rotation = transform.rotation.eulerAngles;

            if (dirx < 0 && diry == 0) { //left
                scale.x = scale.x * -1;
                scale.y = scale.y * -1;
            } else if (dirx == 0 && diry < 0) { //down
                scale.y = scale.y * -1;
            } else if (dirx == 0 && diry > 0) { //up
                scale.x = scale.x * -1;
            } else if (dirx > 0 && diry > 0) { //right up
                rotation.z = 0f;
            } else if (dirx > 0 && diry < 0) { //right down
                rotation.z = -90f;
            } else if (dirx < 0 && diry > 0) { //left up
                scale.x = scale.x * -1;
                scale.y = scale.y * -1;
                rotation.z = -90f;
            } else if (dirx < 0 && diry < 0) { //left down
                scale.x = scale.x * -1;
                scale.y = scale.y * -1;
                rotation.z = 0f;
            }

            transform.localScale = scale;
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