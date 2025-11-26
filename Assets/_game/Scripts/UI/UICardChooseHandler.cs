using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class UICardChoiceWindow : MonoBehaviour {

        public GameObject optionTemplate;
        public Transform optionsContainer;

        LevelUpManager levelUpManager;
        List<GameObject> spawnedOptions = new List<GameObject>();

        void Awake() {
            foreach (Transform child in optionsContainer) {
                Destroy(child.gameObject);
            }
        }

        // public void ShowChoices(List<BaseCardSO> cardChoices, LevelUpManager manager) {
        //     levelUpManager = manager;
        //     gameObject.SetActive(true);

        //     foreach (var oldOption in spawnedOptions) {
        //         Destroy(oldOption);
        //     }
        //     spawnedOptions.Clear();

        //     foreach (var cardData in cardChoices) {
        //         GameObject optionGO = Instantiate(optionTemplate, optionsContainer);
        //         optionGO.SetActive(true);
        //         spawnedOptions.Add(optionGO);

        //         Image icon = optionGO.GetComponent<Image>();
        //         icon.sprite = cardData.icon;

        //         Button button = optionGO.GetComponent<Button>();
        //         if (button) {
        //             button.transition = Selectable.Transition.ColorTint;
        //             button.onClick.RemoveAllListeners();
        //             button.onClick.AddListener(() => OnChoiceMade(cardData));
        //         }
        //     }
        // }

        // private void OnChoiceMade(BaseCardSO chosenCard) {
        //     if (levelUpManager != null) {
        //         levelUpManager.OnCardChosen(chosenCard);
        //     }
        // }
    }
}