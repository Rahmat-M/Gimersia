using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class DeckUIHandler : MonoBehaviour {

        [Header("References")]
        [SerializeField] DeckManager deckManager;
        [SerializeField] PlayerCardManager playerCardManager;

        [Header("UI Elements")]
        [SerializeField] Image slot1Image;
        [SerializeField] Image slot2Image;
        [SerializeField] Transform queueContainer;
        [SerializeField] GameObject queueCardPrefab;
        [SerializeField] Image ultimateSlotImage;
        [SerializeField] GameObject ultimateGlowEffect;
        [SerializeField] Image reloadProgressImage;
        [SerializeField] Image ultimateCooldownImage;

        [Header("Settings")]
        [SerializeField] float animationDuration = 0.3f;
        [SerializeField] Color emptySlotColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] Color filledSlotColor = Color.white;

        private List<Image> queueImages = new List<Image>();

        void Start() {
            if (deckManager == null) deckManager = FindFirstObjectByType<DeckManager>();
            if (playerCardManager == null) playerCardManager = FindFirstObjectByType<PlayerCardManager>();

            // Subscribe to Events
            if (deckManager != null) {
                deckManager.OnSlot1Changed.AddListener(UpdateSlot1);
                deckManager.OnSlot2Changed.AddListener(UpdateSlot2);
                deckManager.OnQueueChanged += UpdateQueue;
                deckManager.OnReloadStart.AddListener(ShowReload);
            }

            if (playerCardManager != null) {
                playerCardManager.OnUltimateReady += OnUltimateReady;
                playerCardManager.OnUltimateUsed += OnUltimateUsed;
                playerCardManager.OnUltimateCooldownStart.AddListener(ShowUltimateCooldown);
            }

            InitializeQueueUI();

            if (reloadProgressImage != null) reloadProgressImage.gameObject.SetActive(false);
            if (ultimateCooldownImage != null) ultimateCooldownImage.gameObject.SetActive(false);
        }

        void OnDestroy() {
            if (deckManager != null) {
                deckManager.OnSlot1Changed.RemoveListener(UpdateSlot1);
                deckManager.OnSlot2Changed.RemoveListener(UpdateSlot2);
                deckManager.OnQueueChanged -= UpdateQueue;
                deckManager.OnReloadStart.RemoveListener(ShowReload);
            }

            if (playerCardManager != null) {
                playerCardManager.OnUltimateReady -= OnUltimateReady;
                playerCardManager.OnUltimateUsed -= OnUltimateUsed;
                playerCardManager.OnUltimateCooldownStart.RemoveListener(ShowUltimateCooldown);
            }
        }

        void ShowReload(float duration) {
            if (reloadProgressImage == null) return;

            reloadProgressImage.gameObject.SetActive(true);
            reloadProgressImage.fillAmount = 0;

            Tween.Custom(0, 1, duration, onValueChange: (val) => reloadProgressImage.fillAmount = val)
                .OnComplete(() => reloadProgressImage.gameObject.SetActive(false));
        }

        void ShowUltimateCooldown(float duration) {
            if (ultimateCooldownImage == null) return;

            ultimateCooldownImage.gameObject.SetActive(true);
            ultimateCooldownImage.fillAmount = 1;

            Tween.Custom(1, 0, duration, onValueChange: (val) => ultimateCooldownImage.fillAmount = val)
                .OnComplete(() => ultimateCooldownImage.gameObject.SetActive(false));
        }

        // --- Slot Updates ---

        void UpdateSlot1(CardData card) {
            AnimateSlotUpdate(slot1Image, card);
        }

        void UpdateSlot2(CardData card) {
            AnimateSlotUpdate(slot2Image, card);
        }

        void AnimateSlotUpdate(Image slotImage, CardData card) {
            if (slotImage == null) return;

            // "Wet Paint" Effect: Scale down slightly then pop up
            Sequence.Create()
                .Chain(Tween.Scale(slotImage.transform, Vector3.one * 0.8f, 0.1f))
                .ChainCallback(() => {
                    if (card != null) {
                        slotImage.sprite = card.icon;
                        slotImage.color = filledSlotColor;
                        slotImage.enabled = true; // Fix: Ensure Image component is enabled
                    } else {
                        slotImage.sprite = null; // Or default empty sprite
                        slotImage.color = emptySlotColor;
                    }
                })
                .Chain(Tween.Scale(slotImage.transform, Vector3.one, 0.2f, Ease.OutBack));
        }

        // --- Queue Updates ---

        void InitializeQueueUI() {
            // Create pool of queue images
            foreach (Transform child in queueContainer) Destroy(child.gameObject);
            queueImages.Clear();

            for (int i = 0; i < 3; i++) { // Show next 3 cards
                GameObject obj = Instantiate(queueCardPrefab, queueContainer);
                Image img = obj.GetComponent<Image>();
                if (img) {
                    img.color = new Color(1, 1, 1, 0.5f); // Faded
                    queueImages.Add(img);
                }
            }
        }

        void UpdateQueue(List<CardData> queue) {
            for (int i = 0; i < queueImages.Count; i++) {
                if (i < queue.Count) {
                    queueImages[i].sprite = queue[i].icon;
                    queueImages[i].gameObject.SetActive(true);
                    queueImages[i].enabled = true; // Fix: Ensure Image component is enabled

                    // Animate "Flow"
                    Tween.PunchScale(queueImages[i].transform, Vector3.one * 0.1f, 0.2f);
                } else {
                    queueImages[i].gameObject.SetActive(false);
                }
            }
        }

        // --- Ultimate Updates ---

        void OnUltimateReady() {
            UpdateUltimateUI(true);
        }

        void OnUltimateUsed() {
            UpdateUltimateUI(false);
        }

        void UpdateUltimateUI(bool isReady) {
            if (ultimateSlotImage == null) return;

            if (isReady) {
                ultimateSlotImage.color = Color.white;
                if (ultimateGlowEffect) ultimateGlowEffect.SetActive(true);

                // Pulse Animation
                Tween.Scale(ultimateSlotImage.transform, 1.1f, 0.5f, Ease.InOutSine, -1, CycleMode.Yoyo);
            } else {
                ultimateSlotImage.color = Color.gray;
                if (ultimateGlowEffect) ultimateGlowEffect.SetActive(false);

                // Stop Pulse
                Tween.StopAll(ultimateSlotImage.transform);
                ultimateSlotImage.transform.localScale = Vector3.one;
            }
        }
    }
}