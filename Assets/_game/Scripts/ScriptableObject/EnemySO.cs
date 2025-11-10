using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Enemy", menuName = "LittaleData/Enemy")]
    public class EnemySO : ScriptableObject {

        //Base stats for the enemy
        [SerializeField] float moveSpeed;
        public float MoveSpeed { get { return moveSpeed; } }

        [SerializeField] float maxHealth;
        public float MaxHealth { get { return maxHealth; } }

        [SerializeField] float damage;
        public float Damage { get { return damage; } }

    }
}
