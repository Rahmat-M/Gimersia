using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class PlayerHealth : MonoBehaviour {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        private float currentHealth;

        [Header("Shield/Defense")]
        private float currentShield = 0f;
        private float shieldDecayTimer = 0f;

        [Header("UI References")]
        public Slider healthBar;
        public Text healthText;
        public Slider shieldBar;

        // Events untuk trigger reaction deck
        public delegate void HealthChangedHandler(float currentHP, float maxHP, float percentage);
        public event HealthChangedHandler OnHealthChanged;

        public delegate void DamagedHandler(float damageAmount);
        public event DamagedHandler OnDamaged;

        public delegate void HealedHandler(float healAmount);
        public event HealedHandler OnHealed;

        public delegate void DeathHandler();
        public event DeathHandler OnDeath;

        void Start() {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }

        void Update() {
            // Shield decay over time
            if (currentShield > 0 && shieldDecayTimer > 0) {
                shieldDecayTimer -= Time.deltaTime;
                if (shieldDecayTimer <= 0) {
                    currentShield = 0;
                    UpdateHealthUI();
                }
            }
        }

        public void TakeDamage(float damage) {
            if (currentHealth <= 0) return;

            float actualDamage = damage;

            // Shield absorbs damage first
            if (currentShield > 0) {
                if (currentShield >= damage) {
                    currentShield -= damage;
                    actualDamage = 0;
                    Debug.Log($"[PlayerHealth] Shield absorbed {damage} damage. Shield remaining: {currentShield}");
                } else {
                    actualDamage = damage - currentShield;
                    currentShield = 0;
                    Debug.Log($"[PlayerHealth] Shield broken! Taking {actualDamage} damage to health");
                }
            }

            if (actualDamage > 0) {
                currentHealth -= actualDamage;
                currentHealth = Mathf.Max(0, currentHealth);

                Debug.Log($"[PlayerHealth] Took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");

                OnDamaged?.Invoke(actualDamage);
            }

            UpdateHealthUI();

            float percentage = (currentHealth / maxHealth) * 100f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth, percentage);

            if (currentHealth <= 0) {
                Die();
            }
        }

        public void Heal(float amount) {
            if (currentHealth <= 0) return;

            float oldHealth = currentHealth;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            float actualHeal = currentHealth - oldHealth;

            Debug.Log($"[PlayerHealth] Healed {actualHeal}. Health: {currentHealth}/{maxHealth}");

            OnHealed?.Invoke(actualHeal);
            UpdateHealthUI();

            float percentage = (currentHealth / maxHealth) * 100f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth, percentage);
        }

        public void AddShield(float amount, float duration) {
            currentShield += amount;
            shieldDecayTimer = duration;

            Debug.Log($"[PlayerHealth] Shield added: {amount} for {duration}s. Total shield: {currentShield}");
            UpdateHealthUI();
        }

        void Die() {
            Debug.Log("[PlayerHealth] Player died!");
            OnDeath?.Invoke();
            // Add death logic here (game over, respawn, etc)
        }

        void UpdateHealthUI() {
            if (healthBar != null) {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }

            if (healthText != null) {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            }

            if (shieldBar != null) {
                shieldBar.gameObject.SetActive(currentShield > 0);
                shieldBar.maxValue = maxHealth;
                shieldBar.value = currentShield;
            }
        }

        // Getters
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => (currentHealth / maxHealth) * 100f;
        public float GetCurrentShield() => currentShield;
        public bool IsAlive() => currentHealth > 0;
    }
}