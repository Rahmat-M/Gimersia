using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class ExperienceUIHandler : MonoBehaviour {

        public PlayerStats characterStats;

        public Image xpBarFill;
        public TMP_Text levelText;

        public string levelPrefix = "Level ";

        void Start() {
            if (characterStats == null) return;
            characterStats.OnExperienceChanged.AddListener(UpdateExperienceUI);
            UpdateExperienceUI(characterStats.Experience);
        }

        public void UpdateExperienceUI(float currentXP) {
            int currentLevel = characterStats.level;
            int maxXP = characterStats.experienceCap;

            levelText.text = levelPrefix + currentLevel.ToString();

            float fillAmount = 0f;
            if (maxXP > 0) fillAmount = currentXP / maxXP;

            xpBarFill.fillAmount = fillAmount;
        }

    }
}