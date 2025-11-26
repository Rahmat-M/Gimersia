using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Littale {
    public class LevelUpManager : MonoBehaviour {

        [Header("UI References")]
        public GameObject choiceWindow;
        public Button option1Button;
        public Button option2Button;
        public Button option3Button;
        public TMP_Text pendingLevelsText;

        PlayerStats characterStats;
        List<PlayerStats.LevelUpBonus> currentOptions = new List<PlayerStats.LevelUpBonus>();

        void Awake() {
            characterStats = FindFirstObjectByType<PlayerStats>();
            
            // Setup Listeners
            if (option1Button) option1Button.onClick.AddListener(() => OnOptionChosen(0));
            if (option2Button) option2Button.onClick.AddListener(() => OnOptionChosen(1));
            if (option3Button) option3Button.onClick.AddListener(() => OnOptionChosen(2));
        }

        public void StartReflectionPhase() {
            if (characterStats == null) return;

            if (characterStats.pendingLevels > 0) {
                ShowLevelUpOptions();
            } else {
                // No levels left, proceed to Draft
                CloseLevelUpWindow();
                DraftManager draft = FindFirstObjectByType<DraftManager>();
                if (draft) draft.ShowDraft();
            }
        }

        void ShowLevelUpOptions() {
            if (choiceWindow) choiceWindow.SetActive(true);
            Time.timeScale = 0f; // Pause Game
            
            if (pendingLevelsText) pendingLevelsText.text = $"LEVEL UP! ({characterStats.pendingLevels} Remaining)";

            GenerateOptions();
            UpdateUI();
        }

        void GenerateOptions() {
            currentOptions.Clear();
            
            // Determine Pool: Rare every 5 levels?
            bool isRare = (characterStats.level % 5 == 0);
            
            // Common Pool: Attack, Health, Wealth, Mana
            List<PlayerStats.LevelUpBonus> pool = new List<PlayerStats.LevelUpBonus> {
                PlayerStats.LevelUpBonus.Attack,
                PlayerStats.LevelUpBonus.Health,
                PlayerStats.LevelUpBonus.Wealth,
                PlayerStats.LevelUpBonus.Mana
            };

            // Rare Pool: Crit, Speed
            if (isRare) {
                pool.Add(PlayerStats.LevelUpBonus.Crit);
                pool.Add(PlayerStats.LevelUpBonus.Speed);
            }

            // Pick 3 Random
            for (int i = 0; i < 3; i++) {
                PlayerStats.LevelUpBonus pick = pool[Random.Range(0, pool.Count)];
                currentOptions.Add(pick);
            }
        }

        void UpdateUI() {
            SetButton(option1Button, currentOptions[0]);
            SetButton(option2Button, currentOptions[1]);
            SetButton(option3Button, currentOptions[2]);
        }

        void SetButton(Button btn, PlayerStats.LevelUpBonus type) {
            if (!btn) return;
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt) {
                switch (type) {
                    case PlayerStats.LevelUpBonus.Attack: txt.text = "Bold Stroke\n+5 ATK"; break;
                    case PlayerStats.LevelUpBonus.Health: txt.text = "Thick Paper\n+10 HP"; break;
                    case PlayerStats.LevelUpBonus.Wealth: txt.text = "Inspiration\n+50 Gold"; break;
                    case PlayerStats.LevelUpBonus.Mana: txt.text = "Vibrant Color\n+5% Mana Regen"; break;
                    case PlayerStats.LevelUpBonus.Crit: txt.text = "Critical Eye\n+5% Crit"; break;
                    case PlayerStats.LevelUpBonus.Speed: txt.text = "Swift Hand\n+10% Speed"; break;
                }
            }
        }

        public void OnOptionChosen(int index) {
            if (characterStats != null && index < currentOptions.Count) {
                characterStats.ApplyLevelUpBonus(currentOptions[index]);
                characterStats.pendingLevels--;
                StartReflectionPhase(); // Loop
            }
        }

        public void CloseLevelUpWindow() {
            if (choiceWindow) choiceWindow.SetActive(false);
            Time.timeScale = 1f; // Resume Game
        }
    }
}