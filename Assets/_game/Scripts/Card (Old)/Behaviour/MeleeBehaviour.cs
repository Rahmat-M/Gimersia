using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    /// <summary>
    /// Base script of all melee behaviours [To be placed on a prefab of a weapon that is a melee]
    /// </summary>
    public class MeleeBehaviour : MonoBehaviour {

        public MainCardSO mainCardData;
        public float destroyAfterSeconds;

        protected Vector3 direction;
        List<GameObject> markedEnemies;

        // Current stats
        protected float currentDamage;
        protected float currentSpeed;

        void Awake() {
            markedEnemies = new List<GameObject>();
            currentDamage = mainCardData.Damage;
            currentSpeed = mainCardData.Speed;
        }

        public float GetCurrentDamage() {
            return currentDamage *= FindFirstObjectByType<PlayerStats>().Actual.might;
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
            if (col.gameObject.CompareTag("Enemy") && !markedEnemies.Contains(col.gameObject)) {
                if (col.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyStats)) {
                    enemyStats.TakeDamage(GetCurrentDamage(), transform.position);
                    markedEnemies.Add(col.gameObject);
                }
            }
        }

    }
}