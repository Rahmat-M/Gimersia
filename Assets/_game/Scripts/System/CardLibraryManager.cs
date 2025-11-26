using UnityEngine;

namespace Littale {
    public class CardLibraryManager : MonoBehaviour {

        [Header("References")]
        // [SerializeField] CardInventory inventory;
        [SerializeField] GameObject libraryPanel;

        [Header("UI Prefab")]
        [SerializeField] GameObject cardUIPrefab;

        [Header("Containers (Scroll View Content)")]
        [Tooltip("Tempat spawn kartu Main Deck")]
        [SerializeField] Transform mainDeckContainer;

        [Tooltip("Tempat spawn kartu Active & Reactive")]
        [SerializeField] Transform abilityDeckContainer;

        [Tooltip("Tempat spawn kartu Passive")]
        [SerializeField] Transform passiveDeckContainer;

        void Start() {
            // if (inventory == null) inventory = FindFirstObjectByType<CardInventory>();

            libraryPanel.SetActive(false);
        }

        // public void OpenLibrary() {
        //     libraryPanel.SetActive(true);
        //     RefreshUI();
        // }

        public void CloseLibrary() {
            libraryPanel.SetActive(false);
        }

        // public void RefreshUI() {
        //     ClearContainer(mainDeckContainer);
        //     ClearContainer(abilityDeckContainer);
        //     ClearContainer(passiveDeckContainer);

        //     if (inventory.GetCardSlots().Count > 0) {
        //         foreach (var card in inventory.GetCardSlots()) {
        //             SpawnCard(card.cardData, mainDeckContainer);
        //         }
        //     } else {
        //         SpawnEmptyPlaceholder("No Card", mainDeckContainer);
        //     }

        //     if (inventory.HasReactiveCard()) {
        //         SpawnCard(inventory.GetReactiveCard().cardData, abilityDeckContainer);
        //     } else {
        //         SpawnEmptyPlaceholder("No Card", abilityDeckContainer);
        //     }

        //     if (inventory.HasActiveCard()) {
        //         SpawnCard(inventory.GetActiveCard().cardData, abilityDeckContainer);
        //     } else {
        //         SpawnEmptyPlaceholder("No Card", abilityDeckContainer);
        //     }

        //     if (inventory.GetPassiveCardSlots().Count > 0) {
        //         foreach (var card in inventory.GetPassiveCardSlots()) {
        //             SpawnCard(card.cardData, passiveDeckContainer);
        //         }
        //     } else {
        //         SpawnEmptyPlaceholder("No Card", passiveDeckContainer);
        //     }
        // }

        // void SpawnCard(BaseCardSO cardController, Transform container) {
        //     GameObject obj = Instantiate(cardUIPrefab, container);
        //     LibraryCardUI ui = obj.GetComponent<LibraryCardUI>();
        //     if (ui != null) {
        //         ui.Initialize(cardController);
        //     }
        // }

        void SpawnEmptyPlaceholder(string label, Transform container) {
            GameObject obj = Instantiate(cardUIPrefab, container);
            LibraryCardUI ui = obj.GetComponent<LibraryCardUI>();
            if (ui != null) {
                ui.SetupEmpty(label);
            }
        }

        void ClearContainer(Transform container) {
            foreach (Transform child in container) {
                Destroy(child.gameObject);
            }
        }
    }
}