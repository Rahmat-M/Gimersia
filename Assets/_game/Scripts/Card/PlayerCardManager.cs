using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class PlayerCardManager : MonoBehaviour {
        [Header("Configuration")]
        public Transform firePoint;
        public DeckManager deckManager;

        [Header("Stats")]
        public float maxMana = 100;
        public float CurrentMana { get; private set; }
        public float manaRegenRate = 5f; // Regen pasif (opsional)

        [Header("Active Loadout")]
        public CardData anchorCard;   // Senjata Utama (Auto)
        public CardData ultimateCard; // Jurus Pamungkas (Manual)
        public List<CardData> passiveArtifacts; // Pasif

        // State Internal
        private float anchorCooldownTimer;
        private float ultimateCooldownTimer;
        private bool isUltimateReady = true;

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
            HandleInputs();
            HandleCooldowns();
        }

        // --- 1. LOGIKA ANCHOR (AUTO-ATTACK) ---
        void HandleAnchorAutoAttack() {
            if (anchorCard == null) return;

            anchorCooldownTimer -= Time.deltaTime;
            if (anchorCooldownTimer <= 0) {
                UseCard(anchorCard);

                // Generate Mana apabila menyerang dengan Anchor
                // Atau mungkin bisa dibuat efek khusus "ManaGainEffect" di dalam kartu
                RegenMana(5);

                anchorCooldownTimer = anchorCard.cooldown;
            }
        }

        // --- 2. LOGIKA INPUT (REVOLVER & ULTIMATE) ---
        void HandleInputs() {
            // Input untuk Revolver Skills (Q & E)
            if (Keyboard.current.qKey.wasPressedThisFrame) deckManager.TryUseCard(1);
            if (Keyboard.current.eKey.wasPressedThisFrame) deckManager.TryUseCard(2);

            // Input untuk Ultimate (R)
            if (Keyboard.current.rKey.wasPressedThisFrame) {
                TryUseUltimate();
            }
        }

        void TryUseUltimate() {
            if (ultimateCard != null && isUltimateReady) {
                UseCard(ultimateCard);
                isUltimateReady = false;
                ultimateCooldownTimer = ultimateCard.cooldown;
                Debug.Log("ULTIMATE UNLEASHED!");
            } else {
                Debug.Log("Ultimate Cooldown!");
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
        public void UseCard(CardData card) {
            // Jika kartu memiliki proyektil
            if (card.projectilePrefab != null) {
                Vector3 spawnPos = firePoint.position;
                Vector3 aimDirection = GetAimDirection(card.type);
                GameObject newObj = Instantiate(card.projectilePrefab, spawnPos, Quaternion.identity);

                var controller = newObj.GetComponent<ProjectileController>();
                if (controller != null) {
                    controller.Initialize(card.effects, this.gameObject, aimDirection);
                }
            }
            // Sebaliknya, langsung terapkan efeknya
            else {
                ApplyEffectsImmediate(card);
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