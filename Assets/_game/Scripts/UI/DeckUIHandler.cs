using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class DeckUIHandler : MonoBehaviour {

        DeckManager deckManager;
        PlayerStats characterStats;

        [Header("Card References")]
        [SerializeField] GameObject cardUIPrefab;
        [SerializeField] Transform mainDeck;

        List<Transform> mainDeckSlots = new List<Transform>();
        Dictionary<CardController, GameObject> cardVisuals = new Dictionary<CardController, GameObject>();

        void Start() {
            deckManager = FindFirstObjectByType<DeckManager>();
            characterStats = FindFirstObjectByType<PlayerStats>();

            InitializeCardVisuals();
            // deckManager.OnCardDrawn += HandleCardDrawn;
            // deckManager.OnCardPlayed += HandleCardPlayed;
        }

        void InitializeCardVisuals() {
            if (mainDeck == null || cardUIPrefab == null) {
                Debug.LogWarning("[DeckUIHandler] mainDeck or cardUIPrefab is not assigned.");
                return;
            }

            foreach (Transform child in mainDeck) { // Clear existing visuals
                Destroy(child.gameObject);
            }

            for (int i = 0; i < characterStats.Actual.handSize; i++) {
                GameObject uiCard = Instantiate(cardUIPrefab, mainDeck);
                uiCard.name = $"CardUI_Slot_{i}";
                uiCard.SetActive(false); // Initially inactive
                mainDeckSlots.Add(uiCard.transform);
            }
        }

        void HandleCardDrawn(CardController card, int handIndex) {
            if (handIndex < 0 || handIndex >= mainDeckSlots.Count) {
                Debug.LogError($"[DeckUIHandler] Invalid handIndex: {handIndex}");
                return;
            }

            Transform cardSlot = mainDeckSlots[handIndex];
            Image img = cardSlot.GetComponent<Image>();
            img.sprite = card.cardData.icon;
            img.color = Color.white; // Ensure to reset color in case it was changed

            GameObject uiCard = cardSlot.gameObject;
            cardVisuals[card] = uiCard;

            StartCoroutine(AnimateCardAppearance(cardSlot.gameObject, 0.2f));
        }

        void HandleCardPlayed(CardController card, float duration) {
            if (cardVisuals.TryGetValue(card, out GameObject uiCard)) {
                SoundManager.Instance.Play("card_played");
                StartCoroutine(PlayAndDiscardAnimation(uiCard, duration));

                cardVisuals.Remove(card);
            }
        }

        IEnumerator AnimateCardAppearance(GameObject cardObj, float duration = 0.2f) {
            if (cardObj == null) yield break;

            RectTransform rect = cardObj.GetComponent<RectTransform>();
            CanvasGroup cg = cardObj.GetComponent<CanvasGroup>() ?? cardObj.AddComponent<CanvasGroup>();

            rect.localScale = Vector3.one * 0.7f;
            cg.alpha = 0f;
            cardObj.SetActive(true);

            float t = 0f;
            while (t < 1f) {
                if (cardObj == null) yield break;
                t += Time.deltaTime / duration;

                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                rect.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one, smoothT);
                cg.alpha = smoothT;

                yield return null;
            }

            // Pastikan berakhir di state yang benar
            if (rect != null) rect.localScale = Vector3.one;
            if (cg != null) cg.alpha = 1f;
        }

        IEnumerator AnimateCardDisappearance(GameObject cardObj, float duration = 0.15f) {
            if (cardObj == null) yield break;

            RectTransform rect = cardObj.GetComponent<RectTransform>();
            CanvasGroup cg = cardObj.GetComponent<CanvasGroup>() ?? cardObj.AddComponent<CanvasGroup>();

            Vector3 startScale = rect.localScale;
            Vector3 endScale = Vector3.one * 0.7f;

            float t = 0f;
            while (t < 1f) {
                if (cardObj == null) yield break;
                t += Time.deltaTime / duration;

                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                rect.localScale = Vector3.Lerp(startScale, endScale, smoothT);
                cg.alpha = 1f - smoothT;

                yield return null;
            }

            if (cardObj != null) cardObj.SetActive(false);
        }

        IEnumerator PlayAndDiscardAnimation(GameObject uiCard, float totalDuration) {
            float highlightTime = totalDuration * 0.7f;
            float discardTime = totalDuration * 0.3f;

            highlightTime = Mathf.Max(highlightTime, 0.1f);
            discardTime = Mathf.Max(discardTime, 0.05f);

            yield return StartCoroutine(Highlight(uiCard, highlightTime));
            yield return StartCoroutine(AnimateCardDisappearance(uiCard, discardTime));
        }

        IEnumerator Highlight(GameObject uiCard, float duration) {
            Image img = uiCard.GetComponent<Image>();
            if (img != null) img.color = Color.yellow;
            yield return new WaitForSeconds(duration);
            if (img != null) img.color = Color.white;
        }

        // void OnDestroy() {
        //     if (deckManager != null) {
        //         deckManager.OnCardDrawn -= HandleCardDrawn;
        //         deckManager.OnCardPlayed -= HandleCardPlayed;
        //     }
        // }

    }
}