using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Littale {
    public class DraftManager : MonoBehaviour {
        [Header("UI References")]
        public GameObject draftWindow;
        public Transform cardContainer;
        public GameObject cardButtonPrefab; // Prefab with Button & Image/Text components

        [Header("Data")]
        public CardDatabase cardDatabase; // Use CardDatabase

        public void ShowDraft() {
            if (draftWindow) draftWindow.SetActive(true);
            Time.timeScale = 0f; // Pause
            GenerateChoices();
        }

        void GenerateChoices() {
            // Clear old choices
            foreach (Transform child in cardContainer) {
                Destroy(child.gameObject);
            }

            // Pick 3 random cards from Database
            List<CardData> choices = new List<CardData>();
            if (cardDatabase != null) {
                choices = cardDatabase.GetRandomCards(3);
            } else {
                Debug.LogWarning("CardDatabase not assigned in DraftManager!");
            }

            // Create Buttons
            foreach (CardData card in choices) {
                GameObject btnObj = Instantiate(cardButtonPrefab, cardContainer);
                // Setup UI (Icon, Name, Description) - Assuming simple setup for now
                // btnObj.GetComponent<Image>().sprite = card.icon;
                // btnObj.GetComponentInChildren<TMP_Text>().text = card.cardName;
                
                btnObj.GetComponent<Button>().onClick.AddListener(() => OnCardPicked(card));
            }
        }

        void OnCardPicked(CardData card) {
            DeckManager deck = FindFirstObjectByType<DeckManager>();
            if (deck) deck.AddCard(card);
            
            CloseDraft();
        }

        void CloseDraft() {
            if (draftWindow) draftWindow.SetActive(false);
            Time.timeScale = 1f; // Resume
        }
    }
}
