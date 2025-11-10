using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class ActiveDeckUI : MonoBehaviour {
        [Header("References")]
        public ActiveDeckSystem activeDeck;

        [Header("UI Elements")]
        public GameObject activeSlotContainer;
        public Image cardIcon;
        public Image cooldownOverlay;
        public Text cooldownText;
        public Text keyBindText; // Display tombol (Space, Q, dll)
        public Image readyGlow; // Glow effect saat ready

        [Header("Channeling UI")]
        public GameObject channelingBar; // Progress bar untuk channeling
        public Image channelingFill; // Fill image untuk progress
        public Text channelingText; // "CHANNELING..."
        public Image channelingOverlay; // Full screen overlay (optional)

        [Header("Animation")]
        public Transform activeDeckPileSlot; // Posisi awal spawn animasi
        public float drawAnimationDuration = 0.8f;

        [Header("Visual Feedback")]
        public Color readyColor = Color.green;
        public Color cooldownColor = Color.gray;
        public float glowPulseSpeed = 2f;

        private bool isCardDrawn = false;
        private CanvasGroup slotCanvasGroup;

        void Start() {
            if (activeDeck != null) {
                activeDeck.OnActiveCardDrawn += HandleActiveCardDrawn;
                activeDeck.OnActiveCardUsed += HandleActiveCardUsed;
                activeDeck.OnCooldownUpdate += HandleCooldownUpdate;
                activeDeck.OnActiveCardReady += HandleActiveCardReady;
                activeDeck.OnChannelingStarted += HandleChannelingStarted;
                activeDeck.OnChannelingUpdate += HandleChannelingUpdate;
                activeDeck.OnChannelingEnded += HandleChannelingEnded;
            }

            // Setup canvas group untuk fade in
            if (activeSlotContainer != null) {
                slotCanvasGroup = activeSlotContainer.GetComponent<CanvasGroup>();
                if (slotCanvasGroup == null)
                    slotCanvasGroup = activeSlotContainer.AddComponent<CanvasGroup>();

                slotCanvasGroup.alpha = 0f;
            }

            // Initialize UI
            if (cooldownOverlay != null) {
                cooldownOverlay.fillAmount = 0;
                cooldownOverlay.enabled = true;
            }

            if (cooldownText != null)
                cooldownText.text = "";

            if (readyGlow != null)
                readyGlow.enabled = false;

            if (keyBindText != null)
                keyBindText.text = activeDeck.activateKey.ToString();

            if (cardIcon != null)
                cardIcon.enabled = false;

            // Initialize channeling UI
            if (channelingBar != null)
                channelingBar.SetActive(false);

            if (channelingOverlay != null)
                channelingOverlay.enabled = false;
        }

        void Update() {
            // Pulse glow effect saat ready
            if (isCardDrawn && readyGlow != null && readyGlow.enabled) {
                float pulse = Mathf.PingPong(Time.time * glowPulseSpeed, 1f);
                Color glowColor = readyGlow.color;
                glowColor.a = 0.5f + (pulse * 0.5f);
                readyGlow.color = glowColor;
            }
        }

        void HandleActiveCardDrawn(CardDataSO card) {
            Debug.Log($"[ActiveDeckUI] Drawing active card: {card.cardName}");
            StartCoroutine(AnimateActiveCardDraw(card));
        }

        IEnumerator AnimateActiveCardDraw(CardDataSO card) {
            // Create temporary card untuk animasi
            GameObject tempCard = new GameObject("TempActiveCard");
            tempCard.transform.SetParent(transform, false);

            RectTransform rect = tempCard.AddComponent<RectTransform>();
            Image img = tempCard.AddComponent<Image>();
            img.sprite = card.icon;
            img.preserveAspect = true;

            CanvasGroup cg = tempCard.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Set size sama dengan slot
            rect.sizeDelta = new Vector2(100, 140);

            // Animate from pile to slot
            Transform startPos = activeDeckPileSlot != null ? activeDeckPileSlot : transform;
            Vector3 endPos = activeSlotContainer.transform.position;

            float t = 0f;
            rect.position = startPos.position;

            while (t < 1f) {
                t += Time.deltaTime / drawAnimationDuration;
                rect.position = Vector3.Lerp(startPos.position, endPos, t);
                cg.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }

            // Destroy temp card
            Destroy(tempCard);

            // Show actual slot
            if (cardIcon != null) {
                cardIcon.sprite = card.icon;
                cardIcon.enabled = true;
            }

            // Fade in slot container
            if (slotCanvasGroup != null) {
                float fadeT = 0f;
                while (fadeT < 1f) {
                    fadeT += Time.deltaTime * 2f;
                    slotCanvasGroup.alpha = fadeT;
                    yield return null;
                }
            }

            isCardDrawn = true;

            // Show ready glow
            if (readyGlow != null) {
                readyGlow.enabled = true;
                readyGlow.color = readyColor;
            }

            Debug.Log("[ActiveDeckUI] Active card draw animation complete!");
        }

        void HandleActiveCardUsed(CardDataSO card) {
            Debug.Log($"[ActiveDeckUI] Active card used: {card.cardName}");

            // Flash effect
            StartCoroutine(FlashEffect());

            // Hide ready glow
            if (readyGlow != null)
                readyGlow.enabled = false;

            // Change color based on skill type
            if (card.isContinuousSkill) {
                // Channeling skill - keep normal color
                if (cardIcon != null)
                    cardIcon.color = Color.white;
            } else {
                // Instant skill - gray for cooldown
                if (cardIcon != null)
                    cardIcon.color = cooldownColor;
            }
        }

        void HandleChannelingStarted(CardDataSO card, float duration) {
            Debug.Log($"[ActiveDeckUI] Channeling started: {card.cardName} for {duration}s");

            // Show channeling bar
            if (channelingBar != null)
                channelingBar.SetActive(true);

            // Show channeling overlay (optional full screen effect)
            if (channelingOverlay != null) {
                channelingOverlay.enabled = true;
                Color overlayColor = channelingOverlay.color;
                overlayColor.a = 0.3f;
                channelingOverlay.color = overlayColor;
            }

            // Set channeling text
            if (channelingText != null)
                channelingText.text = $"CHANNELING: {card.cardName.ToUpper()}";

            // Hide cooldown overlay during channeling
            if (cooldownOverlay != null)
                cooldownOverlay.enabled = false;
        }

        void HandleChannelingUpdate(float remainingTime, float totalDuration) {
            // Update channeling bar fill
            if (channelingFill != null) {
                float fillAmount = 1f - (remainingTime / totalDuration);
                channelingFill.fillAmount = fillAmount;
            }

            // Update channeling text with timer
            if (channelingText != null) {
                channelingText.text = $"CHANNELING... {Mathf.CeilToInt(remainingTime)}s";
            }
        }

        void HandleChannelingEnded() {
            Debug.Log("[ActiveDeckUI] Channeling ended!");

            // Hide channeling bar
            if (channelingBar != null)
                channelingBar.SetActive(false);

            // Hide channeling overlay
            if (channelingOverlay != null)
                channelingOverlay.enabled = false;

            // Show cooldown overlay
            if (cooldownOverlay != null)
                cooldownOverlay.enabled = true;

            // Set card to cooldown color
            if (cardIcon != null)
                cardIcon.color = cooldownColor;
        }

        IEnumerator FlashEffect() {
            if (cardIcon == null) yield break;

            Color originalColor = cardIcon.color;
            cardIcon.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            cardIcon.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            cardIcon.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            cardIcon.color = originalColor;
        }

        void HandleCooldownUpdate(float remainingTime, float totalCooldown) {
            if (cooldownOverlay != null) {
                float fillAmount = remainingTime / totalCooldown;
                cooldownOverlay.fillAmount = fillAmount;
            }

            if (cooldownText != null) {
                if (remainingTime > 0) {
                    cooldownText.text = Mathf.CeilToInt(remainingTime).ToString();
                } else {
                    cooldownText.text = "";
                }
            }
        }

        void HandleActiveCardReady() {
            Debug.Log("[ActiveDeckUI] Active card ready!");

            // Show ready glow
            if (readyGlow != null) {
                readyGlow.enabled = true;
                readyGlow.color = readyColor;
            }

            // Reset color
            if (cardIcon != null)
                cardIcon.color = Color.white;

            // Optional: Play ready sound
            // AudioManager.instance.PlaySound("SkillReady");

            // Optional: Screen shake atau particle effect
            StartCoroutine(ReadyPulseEffect());
        }

        IEnumerator ReadyPulseEffect() {
            // Pulse the slot sedikit saat ready
            Vector3 originalScale = activeSlotContainer.transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;

            float t = 0f;
            while (t < 1f) {
                t += Time.deltaTime * 3f;
                float scale = Mathf.PingPong(t, 0.5f);
                activeSlotContainer.transform.localScale = Vector3.Lerp(originalScale, targetScale, scale);
                yield return null;
            }

            activeSlotContainer.transform.localScale = originalScale;
        }

        // Optional: Visual feedback saat hover
        public void OnSlotHoverEnter() {
            if (activeDeck.IsReady()) {
                // Show tooltip atau highlight
                if (activeSlotContainer != null) {
                    activeSlotContainer.transform.localScale = Vector3.one * 1.1f;
                }
            }
        }

        public void OnSlotHoverExit() {
            if (activeSlotContainer != null) {
                activeSlotContainer.transform.localScale = Vector3.one;
            }
        }
    }
}