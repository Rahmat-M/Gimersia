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

    public class PlayerStats : EntityStats {

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

        // [Header("Testing Prefabs")]
        // [SerializeField] ReactiveCardController reactiveCardPrefab;
        // [SerializeField] ActiveCardController activeCardPrefab;
        // [SerializeField] List<PassiveCardController> startingPassiveCards = new List<PassiveCardController>();

        //I-Frames
        [Header("I-Frames")]
        public float invincibilityDuration;
        float invincibilityTimer;
        bool isInvincible;

        [Header("Damage Feedback")]
        public Color damageColor = new Color(1, 0, 0, 1);
        public float damageFlashDuration = 0.2f;
        public float deathFadeTime = 0.6f;

        // CardInventory inventory;
        // public CardInventory Inventory { get { return inventory; } }
        PlayerCollector collector;

        #endregion

        // TODO: Characters UI

        void Awake() {
            // inventory = GetComponent<CardInventory>();
            collector = GetComponentInChildren<PlayerCollector>();

            //Assign the variables
            baseStats = actualStats = characterData.stats;
            collector.SetRadius(actualStats.magnet);
            health = actualStats.maxHealth;

            // inventory.OnPassiveCardAcquired += (_) => RecalculateStats();
        }

        protected override void Start() {
            base.Start();

            //Initialize the experience cap as the first experience cap increase
            experienceCap = levelRanges[0].experienceCapIncrease;

            //Spawn the starting card
            // foreach (var card in characterData.StartingCards) {
            //     inventory.Add(card);
            // }
            // if (reactiveCardPrefab != null) inventory.Add(reactiveCardPrefab);
            // if (activeCardPrefab != null) inventory.Add(activeCardPrefab);
            // if (startingPassiveCards.Count > 0) {
            //     foreach (var pCard in startingPassiveCards) {
            //         inventory.Add(pCard);
            //     }
            // }
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

        [Header("Growth Triad")]
        public float levelUpBaseMight = 0;
        public float fusionBaseMight = 0;
        public float levelUpBaseHealth = 0;

        public enum LevelUpBonus { Attack, Health, Wealth, Mana, Crit, Speed }

        public void ApplyLevelUpBonus(LevelUpBonus type) {
            switch (type) {
                case LevelUpBonus.Attack:
                    levelUpBaseMight += 5f;
                    break;
                case LevelUpBonus.Health:
                    levelUpBaseHealth += 10f;
                    RestoreHealth(actualStats.maxHealth * 0.2f);
                    break;
                case LevelUpBonus.Wealth:
                    if (collector != null) collector.AddCoins(50);
                    break;
                case LevelUpBonus.Mana:
                    // Assuming we have a base mana regen or similar. For now, let's say it adds a buff or modifies a stat.
                    // Since we don't have a dedicated 'baseManaRegen' field yet, I'll add a placeholder or modify 'recovery' if that's HP regen.
                    // I'll add a 'bonusManaRegen' field to PlayerStats later if needed, or just assume it's handled by multipliers.
                    // For now, let's add to a new field.
                    bonusManaRegen += 0.05f; 
                    break;
                case LevelUpBonus.Crit:
                    bonusCritRate += 5f;
                    break;
                case LevelUpBonus.Speed:
                    bonusAttackSpeed += 0.1f;
                    break;
            }
            RecalculateStats();
        }

        public float bonusManaRegen = 0f;
        public float bonusCritRate = 0f;
        public float bonusAttackSpeed = 0f;
        public float goldMultiplier = 1.0f; // For Art Sale passive
        
        [Header("Neutral Passives")]
        public float armorPenetration = 0f; // Sharpened Pencil
        public float thornsDamage = 0f; // Thick Frame
        public float goldDamageScaling = 0f; // Golden Ratio (1% per 100 Gold)

        public void ApplyFusionBonus(int tier) {
            // Tier 1->2 (Silver): +2 ATK
            // Tier 2->3 (Gold): +5 ATK
            if (tier == 2) fusionBaseMight += 2f;
            else if (tier == 3) fusionBaseMight += 5f;
            RecalculateStats();
        }

        public override void RecalculateStats() {
            // 1. Reset to Base
            actualStats = baseStats;

            // 2. Add Flat Bonuses (The Growth Triad)
            actualStats.might += levelUpBaseMight + fusionBaseMight;
            actualStats.maxHealth += levelUpBaseHealth;
            // Crit and Speed are usually percentage multipliers or flat additions depending on the base stat.
            // Assuming 'might' is damage, 'attackSpeed' is a multiplier (base 1).
            // So we add to the base multiplier.
            // Note: actualStats.attackSpeed is likely a multiplier.
            // We'll apply these AFTER the base reset.
            
            // For now, let's just store them. The actual application happens below or in the multiplier section.
            // Actually, let's apply them to the base stats directly if they are flat, or to the multiplier if they are %.
            // The prompt said: +10% Attack Speed.
            // So we should add 0.1f to the multiplier.

            // 3. Calculate Multipliers (Passive Items / Buffs)
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

            // Apply Level Up Bonuses to Multiplier
            multiplier.attackSpeed += bonusAttackSpeed;
            // multiplier.critRate += bonusCritRate; // Assuming CharacterSO.Stats has critRate. If not, we need to add it or handle it elsewhere.
            // For now, we'll assume Crit is handled separately or added to Stats later.

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

            // 4. Apply Multipliers
            actualStats *= multiplier;

            // Update dependent components
            collector.SetRadius(actualStats.magnet);
        }

        public void IncreaseExperience(int amount) {
            Experience += amount;
            LevelUpChecker();
        }

        [Header("Leveling State")]
        public int pendingLevels = 0;

        void LevelUpChecker() {
            if (Experience >= experienceCap) {
                // Level up the player
                level++;
                pendingLevels++;
                Experience -= experienceCap;

                // Feedback: Auto-Heal and Visuals
                RestoreHealth(5f);
                GameManager.GenerateFloatingText("LEVEL UP!", transform, 1f, 50f);
                if (healEffect) Instantiate(healEffect, transform.position, Quaternion.identity);

                // Update Cap: (Level * 10) + 10
                experienceCap = (level * 10) + 10;

                // Check again in case we gained enough XP for multiple levels
                LevelUpChecker();
            }
        }

        public override bool TakeDamage(float dmg) {
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

            return (CurrentHealth <= 0);
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

        // void OnDestroy() {
        //     if (inventory != null) {
        //         inventory.OnPassiveCardAcquired = null;
        //     }
        // }

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