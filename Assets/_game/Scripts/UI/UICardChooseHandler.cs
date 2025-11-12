using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Littale {
    public class UICardChoiceWindow : MonoBehaviour {
        [Header("Referensi Template")]
        [Tooltip("Prefab/Template untuk satu tombol pilihan kartu")]
        public GameObject optionTemplate;

        [Tooltip("Parent/Container untuk menampung tombol-tombol pilihan")]
        public Transform optionsContainer;

        [Header("Template Paths (Opsional, jika template kompleks)")]
        // Sama seperti UIUpgradeWindow lama Anda, untuk mencari komponen
        public string iconPath = "Icon";
        public string namePath = "Name";
        public string descriptionPath = "Description";
        public string buttonPath = "Button";

        private LevelUpManager levelUpManager;
        private List<GameObject> spawnedOptions = new List<GameObject>();

        void Awake() {
            // Pastikan template disembunyikan di awal
            if (optionTemplate) optionTemplate.SetActive(false);
            gameObject.SetActive(false); // Sembunyikan seluruh window
        }

        // Ini dipanggil oleh LevelUpManager
        public void ShowChoices(List<BaseCardSO> cardChoices, LevelUpManager manager) {
            levelUpManager = manager;
            gameObject.SetActive(true);

            // 1. Bersihkan pilihan lama (jika ada)
            foreach (var oldOption in spawnedOptions) {
                Destroy(oldOption);
            }
            spawnedOptions.Clear();

            // 2. Buat tombol pilihan baru
            foreach (var cardData in cardChoices) {
                // Buat objek UI dari template
                GameObject optionGO = Instantiate(optionTemplate, optionsContainer);
                optionGO.SetActive(true);
                spawnedOptions.Add(optionGO);

                // 3. Isi data kartu ke UI
                // (Gunakan Find() seperti di skrip lama Anda)
                Image icon = optionGO.transform.Find(iconPath).GetComponent<Image>();
                if (icon) icon.sprite = cardData.icon;

                TextMeshProUGUI name = optionGO.transform.Find(namePath).GetComponent<TextMeshProUGUI>();
                if (name) name.text = cardData.cardName;

                TextMeshProUGUI desc = optionGO.transform.Find(descriptionPath).GetComponent<TextMeshProUGUI>();
                if (desc) desc.text = cardData.description;

                // 4. Atur OnClick Button
                Button button = optionGO.transform.Find(buttonPath).GetComponent<Button>();
                if (button) {
                    button.onClick.RemoveAllListeners();
                    // PENTING: Beri tahu LevelUpManager kartu mana yang dipilih
                    button.onClick.AddListener(() => OnChoiceMade(cardData));
                }
            }
        }

        private void OnChoiceMade(BaseCardSO chosenCard) {
            if (levelUpManager != null) {
                levelUpManager.OnCardChosen(chosenCard);
            }
        }
    }
}