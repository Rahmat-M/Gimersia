using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class HealthUIHandler : MonoBehaviour {

        [SerializeField] CharacterStats characterStats;
        [SerializeField] TMP_Text healthText;

        private void Start() {
            characterStats.OnHealthChanged.AddListener(UpdateHealthUI);
        }

        public void UpdateHealthUI(float currentHealth) {
            healthText.text = $"{(int)currentHealth}/{characterStats.Actual.maxHealth}";
        }

    }
}
