using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Main Card", menuName = "Littale Data/Card/Main")]
    public class MainCardSO : BaseCardSO {

        public enum CardType { Melee, Range }

        [Header("Stats")]
        [SerializeField] CardType type;
        public CardType Type { get { return type; } }

        [SerializeField] float damage;
        public float Damage { get { return damage; } }

        [SerializeField] float speed; // For projectiles
        public float Speed { get { return speed; } }

        [SerializeField] int amount; // Number of projectiles spawned
        public int Amount { get { return amount; } }

        [SerializeField] int pierce; // Number of enemies projectile can pierce through
        public int Pierce { get { return pierce; } }

    }
}