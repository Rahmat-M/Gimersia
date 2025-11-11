using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Active Card", menuName = "Littale Data/Card/Active")]
    public class ActiveCardSO : BaseCardSO {

        [Header("Stats")]
        [SerializeField] float cooldown;
        public float Cooldown { get { return cooldown; } }

        [SerializeField] bool isContinuousSkill;
        public bool IsContinuousSkill { get { return isContinuousSkill; } }

        [SerializeField] float effectRadius; // For area effects
        public float EffectRadius { get { return effectRadius; } }

        [SerializeField] int effectAmount;
        public int EffectAmount { get { return effectAmount; } }

        [SerializeField] float effectDuration;
        public float EffectDuration { get { return effectDuration; } }

        [SerializeField] GameObject skillEffectPrefab; // Visual effect prefab for the skill
        public GameObject SkillEffectPrefab { get { return skillEffectPrefab; } }

        [SerializeField] GameObject skillProjectilePrefab; // Projectile prefab for projectile-based skills
        public GameObject SkillProjectilePrefab { get { return skillProjectilePrefab; } }

        [SerializeField] int multiShotCount; // Number of projectiles for MultiShot
        public int MultiShotCount { get { return multiShotCount; } }

    }
}