using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Littale {
    public class HUDController : MonoBehaviour {
        [Header("UI Elements")]
        [SerializeField] TextMeshProUGUI healthText;
        [SerializeField] TextMeshProUGUI coinText;
        [SerializeField] TextMeshProUGUI levelText;

        [Header("References")]
        [SerializeField] PlayerStats playerStats;
        [SerializeField] PlayerCollector playerCollector;

        void Start() {
            // Find the local player
            if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null) {
                if (playerCollector == null) playerCollector = playerStats.GetComponentInChildren<PlayerCollector>();

                // Subscribe to events
                playerStats.OnHealthChanged.AddListener(UpdateHealthUI);
                playerStats.OnLevelChanged.AddListener(UpdateLevelUI);

                if (playerCollector != null) {
                    playerCollector.OnCoinCollected.AddListener(UpdateCoinUI);
                    // Initialize Coin UI
                    UpdateCoinUI(playerCollector.GetCoins());
                }

                // Initialize UI
                UpdateHealthUI(playerStats.CurrentHealth);
                UpdateLevelUI(playerStats.level);
            } else {
                Debug.LogWarning("HUDController: PlayerStats not found!");
            }
        }

        void UpdateHealthUI(float currentHealth) {
            if (healthText != null) {
                if (playerStats != null) {
                    healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(playerStats.Actual.maxHealth)}";
                } else {
                    healthText.text = Mathf.CeilToInt(currentHealth).ToString();
                }
            }
        }

        void UpdateCoinUI(int coins) {
            if (coinText != null) {
                coinText.text = coins.ToString();
            }
        }

        void UpdateLevelUI(int level) {
            if (levelText != null) {
                levelText.text = level.ToString();
            }
        }

        void OnDestroy() {
            if (playerStats != null) {
                playerStats.OnHealthChanged.RemoveListener(UpdateHealthUI);
                playerStats.OnLevelChanged.RemoveListener(UpdateLevelUI);
            }
            if (playerCollector != null) {
                playerCollector.OnCoinCollected.RemoveListener(UpdateCoinUI);
            }
        }
    }
}
