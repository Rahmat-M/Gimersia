using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    public class CharacterStats : EntityStats {

        public UnityAction<float> OnHealthChanged;
        public UnityAction OnKilled;

        [SerializeField] CharacterSO characterData;
        public CharacterSO.Stats baseStats;
        [SerializeField] CharacterSO.Stats actualStats;

        public CharacterSO.Stats Stats { get { return actualStats; } set { actualStats = value; } }
        public CharacterSO.Stats Actual { get { return actualStats; } }

        #region Current Stats Properties
        public float CurrentHealth { get { return health; } set { health = value; } } // TODO: UI update

        [Header("Visuals")]
        public ParticleSystem damageEffect; // If damage is dealt.
        public ParticleSystem blockedEffect; // If armor completely blocks damage.

        //Experience and level of the player
        [Header("Experience/Level")]
        public int experience = 0;
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

        //I-Frames
        [Header("I-Frames")]
        public float invincibilityDuration;
        float invincibilityTimer;
        bool isInvincible;

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
        }

        protected override void Start() {
            base.Start();

            //Initialize the experience cap as the first experience cap increase
            experienceCap = levelRanges[0].experienceCapIncrease;

            //Spawn the starting card
            SpawnCards(characterData.StartingCards);
            if (reactiveCardPrefab != null) SpawnReactiveCard(reactiveCardPrefab);
            if (activeCardPrefab != null) SpawnActiveCard(activeCardPrefab);
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
            experience += amount;
            LevelUpChecker();
        }

        void LevelUpChecker() {
            if (experience >= experienceCap) {
                //Level up the player and reduce their experience by the experience cap
                level++;
                experience -= experienceCap;

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
                dmg -= actualStats.armor;

                if (dmg > 0) {
                    CurrentHealth -= dmg;
                    OnHealthChanged?.Invoke(CurrentHealth);

                    if (damageEffect) Destroy(Instantiate(damageEffect.gameObject, transform.position, Quaternion.identity), damageEffect.main.duration);

                    if (CurrentHealth <= 0) Kill();
                } else {
                    if (blockedEffect) Destroy(Instantiate(blockedEffect.gameObject, transform.position, Quaternion.identity), blockedEffect.main.duration);
                }

                invincibilityTimer = invincibilityDuration;
                isInvincible = true;
            }
        }

        public override void Kill() {
            OnKilled?.Invoke();
            Debug.Log("PLAYER IS DEAD");
        }

        public override void RestoreHealth(float amount) {
            // Only heal the player if their current health is less than their maximum health
            if (CurrentHealth < actualStats.maxHealth) {
                CurrentHealth += amount;

                // Make sure the player's health doesn't exceed their maximum health
                if (CurrentHealth > actualStats.maxHealth) {
                    CurrentHealth = actualStats.maxHealth;
                }
            }
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

        public void SpawnCard(CardController card) {
            //Spawn the starting card
            GameObject spawnedCard = Instantiate(card.gameObject, transform.position, Quaternion.identity);
            spawnedCard.transform.SetParent(transform);    //Set the card to be a child of the player

            inventory.Add(spawnedCard.GetComponent<CardController>());   //Add the card to it's inventory slot
        }

        public void SpawnCards(List<CardController> cards) {
            if (cards == null || cards.Count <= 0) return;
            foreach (var card in cards) {
                SpawnCard(card);
            }
        }

        public void SpawnReactiveCard(ReactiveCardController reactiveCard) {
            //Spawn the reactive card
            GameObject spawnedReactiveItem = Instantiate(reactiveCard.gameObject, transform.position, Quaternion.identity);
            spawnedReactiveItem.transform.SetParent(transform);    //Set the reactive card to be a child of the player

            ReactiveCardController card = spawnedReactiveItem.GetComponent<ReactiveCardController>();
            inventory.Add(card); //Add the reactive card to it's 
        }

        public void SpawnActiveCard(ActiveCardController activeCard) {
            //Spawn the active card
            GameObject spawnedActiveItem = Instantiate(activeCard.gameObject, transform.position, Quaternion.identity);
            spawnedActiveItem.transform.SetParent(transform);    //Set the active card to be a child of the player

            ActiveCardController card = spawnedActiveItem.GetComponent<ActiveCardController>();
            inventory.Add(card); //Add the active card to it's slot
        }

        public void SpawnPassiveCard(PassiveCardController passiveCard) {
            //Spawn the passive card
            GameObject spawnedPassiveItem = Instantiate(passiveCard.gameObject, transform.position, Quaternion.identity);
            spawnedPassiveItem.transform.SetParent(transform);    //Set the passive card to be a child of the player

            inventory.Add(spawnedPassiveItem.GetComponent<PassiveCardController>()); //Add the passive card to it's slot
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