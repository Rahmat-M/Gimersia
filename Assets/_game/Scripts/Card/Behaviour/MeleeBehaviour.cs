using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    /// <summary>
    /// Base script of all melee behaviours [To be placed on a prefab of a weapon that is a melee]
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MeleeBehaviour : MonoBehaviour {

        public MainCardSO mainCardData;
        public float destroyAfterSeconds;

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
            return currentDamage *= FindFirstObjectByType<CharacterStats>().Actual.might;
        }

        protected virtual void Start() {
            Destroy(gameObject, destroyAfterSeconds);
        }

        protected virtual void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.CompareTag("Enemy") && !markedEnemies.Contains(col.gameObject)) {
                if (col.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyStats)) {
                    enemyStats.TakeDamage(GetCurrentDamage());
                    markedEnemies.Add(col.gameObject);
                }
            }
        }

    }
}