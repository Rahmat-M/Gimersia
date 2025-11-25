using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    [System.Serializable]
    public struct LevelGrowth {
        public float damageMultiplierPerLevel;
        public float hpPerLevel;
    }

    public class CharacterStats : EntityStats {

        public UnityEvent<float> OnHealthChanged;
        public UnityEvent<float> OnExperienceChanged;
        public UnityEvent OnKilled;

        [SerializeField] CharacterSO characterData;
        public CharacterSO.Stats baseStats;
        [SerializeField] CharacterSO.Stats actualStats;

        public CharacterSO.Stats Stats { get { return actualStats; } set { actualStats = value; } }
        public CharacterSO.Stats Actual { get { return actualStats; } }

        [Header("Growth")]
        [SerializeField] LevelGrowth growthPerLevel = new LevelGrowth { damageMultiplierPerLevel = 0.1f, hpPerLevel = 5f };

        #region Current Stats Properties
        public float CurrentHealth {
            get { return health; }
            set {
                health = value;
                OnHealthChanged?.Invoke(CurrentHealth); // Invoke health changed event whenever health is set
            }
        }
        public float CurrentArmor {
            get { return actualStats.armor; }
            set { actualStats.armor = value; }
        }

        [Header("Visuals")]
        public ParticleSystem healEffect; // Effect to play when healing.
        public ParticleSystem blockedEffect; // If armor completely blocks damage.

        //Experience and level of the player
        [Header("Experience/Level")]
        int experience = 0;
        public int Experience {
            get { return experience; }
            set {
                experience = value;
                OnExperienceChanged?.Invoke(experience); // Invoke experience changed event whenever experience is set
            }
        }
        public int level = 1;
        public int experienceCap;

        //Class for defining a level range and the corresponding experience cap increase for that range
        [System.Serializable]
        public class LevelRange {
            public int startLevel;
            public int endLevel;
            public int experienceCapIncrease;
        }
        public List<LevelRange> levelRanges;

        [Header("Testing Prefabs")]
        [SerializeField] ReactiveCardController reactiveCardPrefab;
        [SerializeField] ActiveCardController activeCardPrefab;
        [SerializeField] List<PassiveCardController> startingPassiveCards = new List<PassiveCardController>();

        //I-Frames
        [Header("I-Frames")]
        public float invincibilityDuration;
        float invincibilityTimer;
        bool isInvincible;

        [Header("Damage Feedback")]
        public Color damageColor = new Color(1, 0, 0, 1);
        public float damageFlashDuration = 0.2f;
        public float deathFadeTime = 0.6f;

        CardInventory inventory;
        public CardInventory Inventory { get { return inventory; } }
        CharacterCollector collector;

        #endregion

        // TODO: Characters UI

        void Awake() {
            inventory = GetComponent<CardInventory>();
            collector = GetComponentInChildren<CharacterCollector>();

            //Assign the variables
            baseStats = actualStats = characterData.stats;
            collector.SetRadius(actualStats.magnet);
            health = actualStats.maxHealth;

            inventory.OnPassiveCardAcquired += (_) => RecalculateStats();
        }

        protected override void Start() {
            base.Start();

            //Initialize the experience cap as the first experience cap increase
            experienceCap = levelRanges[0].experienceCapIncrease;

            //Spawn the starting card
            foreach (var card in characterData.StartingCards) {
                inventory.Add(card);
            }
            if (reactiveCardPrefab != null) inventory.Add(reactiveCardPrefab);
            if (activeCardPrefab != null) inventory.Add(activeCardPrefab);
            if (startingPassiveCards.Count > 0) {
                foreach (var pCard in startingPassiveCards) {
                    inventory.Add(pCard);
                }
            }
        }

        protected override void Update() {
            base.Update();

            if (invincibilityTimer > 0) {
                invincibilityTimer -= Time.deltaTime;
            } else if (isInvincible) { //If the invincibility timer has reached 0, set the invincibility flag to false
                isInvincible = false;
            }

            Recover();
        }

        public override void RecalculateStats() {
            actualStats = baseStats;
            foreach (var s in inventory.GetPassiveCardSlots()) {
                actualStats += s.GetBoosts();
            }

            float levelDamageMultiplier = 1f + ((level - 1) * growthPerLevel.damageMultiplierPerLevel);
            actualStats.might *= levelDamageMultiplier;

            // Create a variable to store all the cumulative multiplier values.
            CharacterSO.Stats multiplier = new CharacterSO.Stats {
                maxHealth = 1f,
                recovery = 1f,
                armor = 1f,
                moveSpeed = 1f,
                might = 1f,
                attackSpeed = 1f,
                projectileSpeed = 1f,
                magnet = 1f,
                handSize = 1
            };
            foreach (Buff b in activeBuffs) {
                BuffData.Stats bd = b.GetData();
                switch (bd.modifierType) {
                    case BuffData.ModifierType.additive:
                        actualStats += bd.playerModifier;
                        break;
                    case BuffData.ModifierType.multiplicative:
                        multiplier *= bd.playerModifier;
                        break;
                }
            }
            actualStats *= multiplier;

            collector.SetRadius(actualStats.magnet);
        }

        public void IncreaseExperience(int amount) {
            Experience += amount;
            LevelUpChecker();
        }

        void LevelUpChecker() {
            if (Experience >= experienceCap) {
                //Level up the player and reduce their experience by the experience cap
                level++;
                Experience -= experienceCap;

                //Find the experience cap increase for the current level range
                int experienceCapIncrease = 0;
                foreach (LevelRange range in levelRanges) {
                    if (level >= range.startLevel && level <= range.endLevel) {
                        experienceCapIncrease = range.experienceCapIncrease;
                        break;
                    }
                }
                experienceCap += experienceCapIncrease;

                GameManager.Instance.StartLevelUp();
            }
        }

        public override void TakeDamage(float dmg) {
            //If the player is not currently invincible, reduce health and start invincibility
            if (!isInvincible) {
                dmg -= CurrentArmor;

                if (dmg > 0) {
                    CurrentHealth -= dmg;
                    StartCoroutine(DamageFlash());

                    if (CurrentHealth <= 0) Kill();
                } else {
                    CurrentArmor -= dmg; //Reduce armor by the blocked damage amount (negative dmg)
                    if (blockedEffect) Destroy(Instantiate(blockedEffect.gameObject, transform.position, Quaternion.identity), blockedEffect.main.duration);
                }

                invincibilityTimer = invincibilityDuration;
                isInvincible = true;
            }
        }

        IEnumerator DamageFlash() {
            ApplyTint(damageColor);
            yield return new WaitForSeconds(damageFlashDuration);
            RemoveTint(damageColor);
        }

        public override void Kill() {
            OnKilled?.Invoke();
            Debug.Log("PLAYER IS DEAD");
        }

        public override void RestoreHealth(float amount) {
            // Only heal the player if their current health is less than their maximum health
            if (CurrentHealth < actualStats.maxHealth) {
                CurrentHealth += amount;
                if (healEffect) Destroy(Instantiate(healEffect.gameObject, transform.position, Quaternion.identity), healEffect.main.duration);

                // Make sure the player's health doesn't exceed their maximum health
                if (CurrentHealth > actualStats.maxHealth) {
                    CurrentHealth = actualStats.maxHealth;
                }
            }
        }

        public void RestoreArmor(float amount) {
            CurrentArmor += amount;
        }

        void Recover() {
            if (CurrentHealth < actualStats.maxHealth) {
                CurrentHealth += Stats.recovery * Time.deltaTime;

                // Make sure the player's health doesn't exceed their maximum health
                if (CurrentHealth > actualStats.maxHealth) {
                    CurrentHealth = actualStats.maxHealth;
                }
            }
        }

        void OnDestroy() {
            if (inventory != null) {
                inventory.OnPassiveCardAcquired = null;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Damage Player To Quarter Health")]
        void DamagePlayerToQuarterHealth() {
            TakeDamage(CurrentHealth - (actualStats.maxHealth / 4f));
        }

        [ContextMenu("Increase Experience By 100")]
        void IncreaseExperienceBy100() {
            IncreaseExperience(100);
        }

        [ContextMenu("Increase Experience By 500")]
        void IncreaseExperienceBy500() {
            IncreaseExperience(500);
        }
#endif

    }
}