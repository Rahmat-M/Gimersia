using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class HealthUIHandler : MonoBehaviour {

        [SerializeField] PlayerStats characterStats;
        public List<Image> dropVisuals;

        private void Start() {
            characterStats.OnHealthChanged.AddListener(UpdateHealthUI);
        }

        public void UpdateHealthUI(float currentHealth) {
            float healthPercentage = Mathf.Clamp01(currentHealth / characterStats.Stats.maxHealth);
            float totalDropsFill = healthPercentage * dropVisuals.Count;

            for (int i = 0; i < dropVisuals.Count; i++) {
                dropVisuals[i].fillAmount = Mathf.Clamp01(totalDropsFill - i);
            }
        }

    }
}
