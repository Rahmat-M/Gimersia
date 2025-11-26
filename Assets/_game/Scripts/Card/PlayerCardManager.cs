using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Littale {
    public class PlayerCardManager : MonoBehaviour {
        [Header("Configuration")]
        public Transform firePoint;

        [Header("Stats")]
        public float maxMana = 100;
        public float CurrentMana { get; private set; }
        public float manaRegenRate = 5f; // Regen pasif (opsional)
        public int mainAttackLevel = 1;

        [Header("Active Loadout")]
        public CardData anchorCard; // Kartu Anchor (Auto)
        public CardData ultimateCard; // Jurus Pamungkas (Manual)
        public List<CardData> passiveArtifacts; // Pasif

        // State Internal
        private float anchorCooldownTimer;
        private float ultimateCooldownTimer;
        private bool isUltimateReady = true;

        PlayerMovement playerMovement;
        DeckManager deckManager;

        void Awake() {
            playerMovement = GetComponent<PlayerMovement>();
            deckManager = GetComponent<DeckManager>();
        }

        void Start() {
            CurrentMana = maxMana;

            // Aktifkan Efek Pasif (Artifact) di awal game
            foreach (var artifact in passiveArtifacts) {
                ApplyEffectsImmediate(artifact);
            }
        }

        void Update() {
            HandleManaRegen();
            HandleAnchorAutoAttack();
            HandleCooldowns();

            // Sync Main Attack Level with Player Level
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats) mainAttackLevel = stats.level;
        }

        // --- 1. LOGIKA ANCHOR (AUTO-ATTACK) ---
        void HandleAnchorAutoAttack() {
            // "The Reliable Brush"
            // Lv 1: Single Stroke
            // Lv 5: Double Stroke
            // Lv 10: Ink Siphon (+1 Mana on Kill)

            if (anchorCard == null) return;

            anchorCooldownTimer -= Time.deltaTime;
            if (anchorCooldownTimer <= 0) {
                // Execute Attack
                ExecuteMainAttack();

                // Reset Timer
                anchorCooldownTimer = anchorCard.cooldown;
            }
        }

        // --- 2. LOGIKA INPUT (REVOLVER & ULTIMATE) ---

        // Input Q & E via Input System
        public void OnSkillQ(InputAction.CallbackContext ctx) {
            if (ctx.performed) {
                deckManager.TryUseCard(1);
            }
        }

        public void OnSkillE(InputAction.CallbackContext ctx) {
            if (ctx.performed) {
                deckManager.TryUseCard(2);
            }
        }

        // Input R via Input System
        public void OnUltimate(InputAction.CallbackContext ctx) {
            if (ctx.performed) {
                TryUseUltimate();
            }
        }

        void ExecuteMainAttack() {
            // Hitung Logic Level
            bool isDouble = mainAttackLevel >= 5;
            bool isSiphon = mainAttackLevel >= 10;

            // 1. Get Direction & Rotation
            Vector3 attackDir = playerMovement.LastMovementInput;
            if (attackDir == Vector3.zero) attackDir = transform.right; // Fallback

            float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 2. Calculate Offset Position
            float offsetDistance = 1.0f; // Jarak di depan player
            Vector3 spawnPos = transform.position + (attackDir.normalized * offsetDistance);

            // 3. Instantiate Melee Prefab
            GameObject slash = Instantiate(anchorCard.projectilePrefab, spawnPos, rotation);

            // 4. Initialize Controller
            var meleeCtrl = slash.GetComponent<MeleeAttackController>();
            if (meleeCtrl != null) {
                meleeCtrl.Initialize(10, isDouble, isSiphon, this);
            }

            // Jika Lv 5 (Double Stroke), spawn ayunan kedua dengan delay sedikit dan arah berlawanan
            if (isDouble) {
                StartCoroutine(SecondStrokeDelay(isSiphon, rotation, spawnPos));
            }

            // Mana Regen per hit/swing (Basic)
            RegenMana(1);

            // Passive Trigger
            IncrementAttackCounter();
        }

        IEnumerator SecondStrokeDelay(bool siphon, Quaternion originalRotation, Vector3 position) {
            yield return new WaitForSeconds(0.1f);
            // Spawn ayunan balik (Flip 180 derajat)
            Quaternion reverseRot = originalRotation * Quaternion.Euler(0, 0, 180);

            GameObject slash2 = Instantiate(anchorCard.projectilePrefab, position, reverseRot);
            var meleeCtrl = slash2.GetComponent<MeleeAttackController>();
            if (meleeCtrl != null) {
                meleeCtrl.Initialize(10, true, siphon, this);
            }
        }

        void TryUseUltimate() {
            // "Summon: Black Dragon"
            if (ultimateCard != null && isUltimateReady) {
                // Screen Nuke / Panic Button

                // 1. Bullet Clear
                ClearEnemyBullets();

                // 2. Damage All Enemies
                DamageAllEnemies(500); // 500% Damage

                // 3. Push Boss (Logic handled in DamageAllEnemies or separate)

                // Visuals
                if (ultimateCard.projectilePrefab != null) {
                    Instantiate(ultimateCard.projectilePrefab, transform.position, Quaternion.identity);
                }

                isUltimateReady = false;
                ultimateCooldownTimer = ultimateCard.cooldown;
                Debug.Log("ULTIMATE: BLACK DRAGON UNLEASHED!");
            } else {
                Debug.Log("Ultimate Cooldown!");
            }
        }

        void ClearEnemyBullets() {
            var bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
            foreach (var b in bullets) Destroy(b);
        }

        void DamageAllEnemies(int damagePercent) {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies) {
                // Apply Damage
                // e.GetComponent<EnemyHealth>()?.TakeDamage(damagePercent);

                // Push Boss Logic
                // if (e.CompareTag("Boss")) PushBack(e);
            }
        }

        void HandleCooldowns() {
            if (!isUltimateReady) {
                ultimateCooldownTimer -= Time.deltaTime;
                if (ultimateCooldownTimer <= 0) {
                    isUltimateReady = true;
                    Debug.Log("Ultimate Ready!");
                }
            }
        }

        // --- 3. SISTEM EKSEKUSI ---
        public void UseCard(CardData card, CardTier tier, int slotIndex) {
            switch (card.cardName) {
                case "Ink Lance":
                    UseInkLance(card, tier, slotIndex);
                    break;
                case "Splatter Bomb":
                    UseSplatterBomb(card, tier);
                    break;
                case "Tiding Wave":
                    UseTidingWave(card, tier);
                    break;
                case "Binding Trap":
                    UseBindingTrap(card, tier);
                    break;
                case "Shadow Step":
                    UseShadowStep(card, tier);
                    break;
                case "Palette Knife":
                    UsePaletteKnife(card, tier);
                    break;
                case "Eraser Rub":
                    UseEraserRub(card, tier);
                    break;
                case "Sketch Decoy":
                    UseSketchDecoy(card, tier);
                    break;
                case "Still Life: Apple":
                    UseStillLifeApple(card, tier);
                    break;
                case "Water Bucket":
                    UseWaterBucket(card, tier);
                    break;
                default:
                    ApplyEffectsImmediate(card);
                    break;
            }

            // Trigger Passives
            CheckPassivesOnUse(card);
        }

        #region Projectile & Effects
        public void UseInkLance(CardData card, CardTier tier, int slotIndex) {
            // Target: Musuh terjauh atau Boss
            GameObject target = FindFarthestOrBoss();
            Vector3 targetDir = (target != null) ? (target.transform.position - transform.position).normalized : firePoint.right;

            GameObject lance = Instantiate(card.projectilePrefab, firePoint.position, Quaternion.FromToRotation(Vector3.right, targetDir));
            ProjectileController pc = lance.GetComponent<ProjectileController>();

            // Fusion Evolution
            // Bronze: Pierce 3
            // Silver: Infinite Pierce
            // Gold: Killer Art (Reset Cooldown on Kill)

            int pierceCount = (tier == CardTier.Bronze) ? 3 : 999;
            bool isKillerArt = (tier == CardTier.Gold);

            pc.Initialize(card.effects, this.gameObject, targetDir);
            pc.SetPierce(pierceCount);

            if (isKillerArt) {
                pc.OnKill += () => deckManager.ResetCooldown(slotIndex);
            }
        }

        public void UseSplatterBomb(CardData card, CardTier tier) {
            // Target: Mouse Pos
            Vector3 targetPos = GetMouseWorldPosition();

            GameObject bomb = Instantiate(card.projectilePrefab, firePoint.position, Quaternion.identity);
            ProjectileController pc = bomb.GetComponent<ProjectileController>();

            // Fusion Evolution
            bool spawnPuddle = (tier >= CardTier.Silver);
            bool isCluster = (tier == CardTier.Gold);

            // Launch Arcing Projectile
            pc.LaunchTo(targetPos, () => {
                // On Reach: Explode
                // Instantiate Explosion Effect / Damage Area
                if (card.secondaryPrefab) Instantiate(card.secondaryPrefab, targetPos, Quaternion.identity); // Explosion Prefab

                // TODO: Apply Damage in Area (using Physics2D.OverlapCircle)

                if (spawnPuddle) {
                    // Instantiate Puddle
                }
                if (isCluster) {
                    // Spawn 3 mini bombs
                }
                Destroy(bomb);
            });
        }

        public void UseTidingWave(CardData card, CardTier tier) {
            // Fusion Evolution
            Vector3 scale = (tier == CardTier.Gold) ? new Vector3(2, 1, 1) : Vector3.one;

            GameObject wave = Instantiate(card.projectilePrefab, firePoint.position, firePoint.rotation);
            wave.transform.localScale = scale;

            WaveController wc = wave.GetComponent<WaveController>();
            if (wc) wc.Initialize(force: (tier >= CardTier.Bronze ? 10f : 5f), stun: (tier >= CardTier.Silver), this.gameObject);
        }

        public void UseBindingTrap(CardData card, CardTier tier) {
            Vector3 targetPos = GetMouseWorldPosition();

            // Fusion Evolution
            GameObject trap = Instantiate(card.projectilePrefab, targetPos, Quaternion.identity);
            TrapController tc = trap.GetComponent<TrapController>();

            float slowAmount = (tier >= CardTier.Silver) ? 0.8f : 0.4f;
            bool isCorrosive = (tier == CardTier.Gold);

            if (tc) tc.Initialize(slowAmount, isCorrosive);
        }

        public void UseShadowStep(CardData card, CardTier tier) {
            // Fusion Evolution
            Vector3 dashDir = playerMovement != null ? playerMovement.LastMovementInput : Vector3.right;
            if (dashDir == Vector3.zero) dashDir = transform.right;

            if (tier >= CardTier.Silver) {
                // Spawn Decoy at current pos
                if (card.secondaryPrefab != null) Instantiate(card.secondaryPrefab, transform.position, Quaternion.identity);
            }
            // Perform Dash (Assuming PlayerMovement has Dash)
            // playerMovement.Dash(dashDir);
        }

        // --- NEUTRAL CARDS ---

        public void UsePaletteKnife(CardData card, CardTier tier) {
            // Bronze: Sweep (150%)
            // Silver: Bleed
            // Gold: Spin 360

            bool isSpin = (tier == CardTier.Gold);
            bool applyBleed = (tier >= CardTier.Silver);

            if (isSpin) {
                // Instantiate Spin Attack (Circular Hitbox)
                // Assuming secondaryPrefab is the Spin version
                GameObject spin = Instantiate(card.secondaryPrefab, transform.position, Quaternion.identity, transform);
                // Initialize Spin Controller
            } else {
                // Standard Sweep
                GameObject sweep = Instantiate(card.projectilePrefab, firePoint.position, firePoint.rotation);
                // Initialize Sweep Controller
            }
        }

        public void UseEraserRub(CardData card, CardTier tier) {
            // Bronze: Damage
            // Silver: Armor Break
            // Gold: Bullet Clear

            Vector3 targetPos = GetMouseWorldPosition();
            GameObject eraser = Instantiate(card.projectilePrefab, targetPos, Quaternion.identity);
            ZoneController zc = eraser.GetComponent<ZoneController>();

            if (zc) {
                zc.Initialize(ZoneController.ZoneType.Damage, 20f, 1f); // One-shot or short duration
                zc.armorBreak = (tier >= CardTier.Silver);
                zc.clearBullets = (tier == CardTier.Gold);
            }
        }

        public void UseSketchDecoy(CardData card, CardTier tier) {
            // Bronze: Low HP
            // Silver: Explode
            // Gold: Twin

            Vector3 targetPos = GetMouseWorldPosition();
            int count = (tier == CardTier.Gold) ? 2 : 1;

            for (int i = 0; i < count; i++) {
                Vector3 offset = (i == 0) ? Vector3.zero : new Vector3(1, 0, 0);
                GameObject decoy = Instantiate(card.projectilePrefab, targetPos + offset, Quaternion.identity);
                DecoyController dc = decoy.GetComponent<DecoyController>();
                if (dc) dc.Initialize(hp: 50f, explode: (tier >= CardTier.Silver));
            }
        }

        public void UseStillLifeApple(CardData card, CardTier tier) {
            // Bronze: Heal 5/s
            // Silver: Heal 10/s + Def
            // Gold: + Atk

            GameObject apple = Instantiate(card.projectilePrefab, transform.position, Quaternion.identity);
            ZoneController zc = apple.GetComponent<ZoneController>();

            float healAmount = (tier >= CardTier.Silver) ? 10f : 5f;
            if (zc) zc.Initialize(ZoneController.ZoneType.Heal, healAmount, 5f);
        }

        public void UseWaterBucket(CardData card, CardTier tier) {
            // Bronze: Push
            // Silver: Slow
            // Gold: Huge Area

            Vector3 targetPos = GetMouseWorldPosition();
            GameObject bucket = Instantiate(card.projectilePrefab, targetPos, Quaternion.identity);

            if (tier == CardTier.Gold) bucket.transform.localScale *= 2f;

            ZoneController zc = bucket.GetComponent<ZoneController>();
            if (zc) {
                zc.Initialize(ZoneController.ZoneType.Damage, 0f, 3f); // No damage, just status
                zc.applyWet = true;
            }
        }
        #endregion

        // --- Helpers ---
        GameObject FindFarthestOrBoss() {
            // Simplified logic
            return FindNearestEnemy(); // Placeholder
        }

        Vector3 GetMouseWorldPosition() {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f; // Distance from camera
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        int attackCounter = 0;
        void CheckPassivesOnUse(CardData card) {
            // "Dripping Brush" Logic
            // Every 5 attacks -> Free Ink Lance
            // Usually triggered by Main Attack, but if we want it on Skill use too:
            // IncrementAttackCounter(); 
        }

        // Call this from ExecuteMainAttack
        void IncrementAttackCounter() {
            attackCounter++;

            // Check for Dripping Brush Passive
            // We need to check if we have the artifact. For now, let's assume we check the list.
            bool hasDrippingBrush = passiveArtifacts.Exists(x => x.cardName == "Dripping Brush");

            if (hasDrippingBrush && attackCounter >= 5) {
                // Fire Free Ink Lance
                // Find Ink Lance card data (or create a temporary one)
                // For now, we'll just instantiate the projectile if we can find it, or use a default.
                // Better: Look for "Ink Lance" in deck or resources.
                // Simplified: Just fire a projectile if we have a reference.

                // Debug.Log("Dripping Brush Triggered!");
                // TODO: Need reference to Ink Lance prefab/card.

                attackCounter = 0;
            }
        }

        void SpawnProjectile(GameObject prefab, Vector3 position, Vector3 direction, List<CardEffect> effects) {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject newProjectile = Instantiate(prefab, position, rotation);

            ProjectileController controller = newProjectile.GetComponent<ProjectileController>();

            if (controller != null) {
                controller.Initialize(effects, this.gameObject, direction);
            } else {
                Debug.LogWarning($"Prefab {prefab.name} tidak memiliki script ProjectileController!");
            }
        }

        void ApplyEffectsImmediate(CardData card) {
            foreach (var effect in card.effects) {
                effect.Execute(this.gameObject, this.gameObject);
            }
        }

        Vector3 GetAimDirection(CardType type) {
            // A. Jika ANCHOR (Auto-Attack): Cari musuh terdekat
            if (type == CardType.Anchor) {
                GameObject nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null) {
                    return (nearestEnemy.transform.position - transform.position).normalized;
                } else {
                    // Jika tidak ada musuh, tembak lurus saja atau acak
                    return Random.insideUnitCircle.normalized;
                }
            }

            // TODO: B. Jika REVOLVER/SKILL: Ikuti arah hadap player atau Mouse
            // (Contoh: Ikuti arah gerak player terakhir)
            // Asumsi kamu punya variabel lastMoveDirection di script pergerakan player
            // return lastMoveDirection; 

            return Vector3.right;
        }

        GameObject FindNearestEnemy() {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject nearest = null;
            float minDistance = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (GameObject enemy in enemies) {
                float dist = Vector3.Distance(enemy.transform.position, currentPos);
                if (dist < minDistance) {
                    nearest = enemy;
                    minDistance = dist;
                }
            }
            return nearest;
        }

        // --- MANA SYSTEM ---
        void HandleManaRegen() {
            if (CurrentMana < maxMana) {
                CurrentMana += manaRegenRate * Time.deltaTime;
            }
        }

        public void ConsumeMana(int amount) {
            CurrentMana -= amount;
            if (CurrentMana < 0) CurrentMana = 0;
        }

        public void RegenMana(int amount) {
            CurrentMana += amount;
            if (CurrentMana > maxMana) CurrentMana = maxMana;
        }
    }
}