using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class ReactionDeckHandler : MonoBehaviour {

        // CardInventory cardInventory;

        [Header("Card References")]
        [SerializeField] GameObject cardUIPrefab;
        [SerializeField] Transform reactionDeck;

        CardUIRefs uiCardRefs;

        // void Awake() {
        //     cardInventory = FindFirstObjectByType<CardInventory>();

        //     foreach (Transform child in reactionDeck) Destroy(child.gameObject); // Clear existing cards
        //     cardInventory.OnReactiveCardAcquired += HandleCardAcquired;
        // }

        // void SubscribeToCardEvents(ReactiveCardController card) {
        //     card.OnCardPutOnCooldown += HandleCardCooldownStart;
        //     card.OnCooldownTick += HandleCooldownTick;
        //     card.OnCardReady += HandleCardReady;
        // }

        // void HandleCardAcquired(ReactiveCardController card) {
        //     if (reactionDeck == null || cardUIPrefab == null) {
        //         Debug.LogWarning("[ReactionDeckHandler] reactionDeck or cardUIPrefab is not assigned.");
        //         return;
        //     }

        //     GameObject uiCardObj = Instantiate(cardUIPrefab, reactionDeck);
        //     uiCardObj.name = $"ReactionCard_{card.cardData.name}";

        //     uiCardRefs = new CardUIRefs(uiCardObj);

        //     uiCardRefs.mainImage.sprite = card.cardData.icon;
        //     uiCardRefs.mainImage.color = Color.white; // Ensure in ready state

        //     if (uiCardRefs.cooldownOverlay) uiCardRefs.cooldownOverlay.gameObject.SetActive(false);
        //     if (uiCardRefs.cooldownText) uiCardRefs.cooldownText.gameObject.SetActive(false);

        //     SubscribeToCardEvents(card); // Subscribe to card events
        //     StartCoroutine(AnimateCardAppearance(uiCardObj));
        // }

        void HandleCardCooldownStart(int duration) {
            SoundManager.Instance.Play("card_go_cooldown");
            StartCoroutine(AnimateCooldownStart(uiCardRefs, duration));
        }

        void HandleCooldownTick(float progressNormalized, int turnsLeft) {
            var refs = uiCardRefs;

            if (refs.cooldownOverlay) { // normalized 0-1
                refs.cooldownOverlay.fillAmount = progressNormalized;
            }
            if (refs.cooldownText) {
                refs.cooldownText.text = turnsLeft.ToString();
            }
        }

        void HandleCardReady() {
            var refs = uiCardRefs;
            SoundManager.Instance.Play("card_done_cooldown");
            StartCoroutine(AnimateCardReady(refs));
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

            if (rect != null) rect.localScale = Vector3.one;
            if (cg != null) cg.alpha = 1f;
        }

        IEnumerator AnimateCooldownStart(CardUIRefs refs, int duration) {
            yield return StartCoroutine(Highlight(refs.root, Color.yellow, 0.2f));

            if (refs.mainImage) refs.mainImage.color = Color.gray;

            if (refs.cooldownOverlay) {
                refs.cooldownOverlay.gameObject.SetActive(true);
                refs.cooldownOverlay.fillAmount = 1f;
            }
            if (refs.cooldownText) {
                refs.cooldownText.gameObject.SetActive(true);
                refs.cooldownText.text = duration.ToString();
            }
        }

        IEnumerator Highlight(GameObject uiCard, Color highlightColor, float duration) {
            Image img = uiCard.GetComponent<Image>();
            Color originalColor = img.color;

            if (img != null) img.color = highlightColor;
            yield return new WaitForSeconds(duration);
            if (img != null) img.color = originalColor;
        }

        IEnumerator AnimateCardReady(CardUIRefs refs) {
            if (refs.cooldownOverlay) refs.cooldownOverlay.gameObject.SetActive(false);
            if (refs.cooldownText) refs.cooldownText.gameObject.SetActive(false);

            if (refs.mainImage) refs.mainImage.color = Color.cyan;

            if (refs.root) refs.root.transform.localScale = Vector3.one * 1.15f;

            yield return new WaitForSeconds(0.15f);

            if (refs.mainImage) refs.mainImage.color = Color.white;
            if (refs.root) refs.root.transform.localScale = Vector3.one;
        }

    }
}