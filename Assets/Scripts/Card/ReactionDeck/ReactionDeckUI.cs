using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactionDeckUI : MonoBehaviour
{
    [Header("References")]
    public ReactionDeckSystem reactionDeck;

    [Header("UI Elements")]
    public Transform reactionSlotsContainer;
    public GameObject reactionSlotPrefab;

    [Header("Animation")]
    public Transform reactionDeckPileSlot; // Posisi "deck" tempat card keluar (seperti pile di hand deck)
    public float drawAnimationDuration = 0.8f;

    [Header("Replace Card Panel")]
    public GameObject replaceCardPanel;
    public Text replaceCardText;
    public Button[] replaceSlotButtons; // Button untuk pilih slot mana yang mau di-replace

    private List<GameObject> reactionSlotVisuals = new List<GameObject>();
    private CardData pendingReplacementCard; // Card yang menunggu untuk di-replace

    void Start()
    {
        if (reactionDeck != null)
        {
            reactionDeck.OnReactionCardDrawing += HandleReactionCardDrawing;
            reactionDeck.OnReactionCardReplaced += HandleReactionCardReplaced;
            reactionDeck.OnReactionTriggered += HandleReactionTriggered;
        }

        // Jangan initialize slots di sini, biarkan spawn via animasi
        // InitializeReactionSlots();

        if (replaceCardPanel != null)
            replaceCardPanel.SetActive(false);
    }

    void InitializeReactionSlots()
    {
        // Clear existing slots
        foreach (Transform child in reactionSlotsContainer)
            Destroy(child.gameObject);
        reactionSlotVisuals.Clear();

        // Create visual slots
        var slots = reactionDeck.GetReactionSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            CreateReactionSlotVisual(i);
        }

        // Force rebuild layout immediately
        StartCoroutine(RebuildLayoutNextFrame());
    }

    void CreateReactionSlotVisual(int index)
    {
        if (reactionSlotPrefab == null)
        {
            Debug.LogError("[ReactionDeckUI] ReactionSlotPrefab is not assigned!");
            return;
        }

        GameObject slotObj = Instantiate(reactionSlotPrefab, reactionSlotsContainer);
        slotObj.name = $"ReactionSlot_{index}";

        // Setup RectTransform untuk proper layout
        RectTransform rect = slotObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        // Setup slot visual - Improved path finding
        Image cardImage = FindChildImage(slotObj, "CardImage");
        Image cooldownOverlay = FindChildImage(slotObj, "CooldownOverlay");
        Text cooldownText = FindChildText(slotObj, "CooldownText");

        if (cardImage != null)
        {
            cardImage.enabled = false;
            cardImage.color = Color.white;
        }
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0;
            cooldownOverlay.enabled = true;
        }
        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        reactionSlotVisuals.Add(slotObj);
        Debug.Log($"[ReactionDeckUI] Created slot {index} at position {slotObj.transform.position}");
    }

    // Helper methods untuk find components
    Image FindChildImage(GameObject parent, string childName)
    {
        Transform child = parent.transform.Find(childName);
        if (child == null)
        {
            // Try recursive search
            child = FindChildRecursive(parent.transform, childName);
        }
        return child?.GetComponent<Image>();
    }

    Text FindChildText(GameObject parent, string childName)
    {
        Transform child = parent.transform.Find(childName);
        if (child == null)
        {
            child = FindChildRecursive(parent.transform, childName);
        }
        return child?.GetComponent<Text>();
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    IEnumerator RebuildLayoutNextFrame()
    {
        yield return null; // Wait 1 frame
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(reactionSlotsContainer.GetComponent<RectTransform>());
    }

    // Handle reaction card drawing dengan animasi (mirip hand deck)
    void HandleReactionCardDrawing(CardData card, int slotIndex)
    {
        Debug.Log($"[ReactionDeckUI] Drawing reaction card {card.cardName} to slot {slotIndex}");

        // Create slot visual dulu jika belum ada
        while (reactionSlotVisuals.Count <= slotIndex)
        {
            CreateReactionSlotVisual(reactionSlotVisuals.Count);
        }

        // Get target slot position
        GameObject targetSlot = reactionSlotVisuals[slotIndex];

        // Create temporary card visual untuk animasi
        GameObject animCard = Instantiate(reactionSlotPrefab, transform);
        animCard.name = $"AnimCard_{card.cardName}";

        // Set card image
        Image cardImage = FindChildImage(animCard, "CardImage");
        if (cardImage != null)
        {
            cardImage.sprite = card.icon;
            cardImage.enabled = true;
        }

        // Hide cooldown elements during draw
        Image cooldownOverlay = FindChildImage(animCard, "CooldownOverlay");
        Text cooldownText = FindChildText(animCard, "CooldownText");
        if (cooldownOverlay != null) cooldownOverlay.enabled = false;
        if (cooldownText != null) cooldownText.enabled = false;

        // Start draw animation
        Transform startPos = reactionDeckPileSlot != null ? reactionDeckPileSlot : transform;
        StartCoroutine(AnimateReactionCardDraw(animCard, startPos, targetSlot.transform, card, slotIndex));
    }

    IEnumerator AnimateReactionCardDraw(GameObject animCard, Transform startPos, Transform endPos, CardData card, int slotIndex)
    {
        if (animCard == null || startPos == null || endPos == null) yield break;

        RectTransform rect = animCard.GetComponent<RectTransform>();
        if (rect == null) yield break;

        // Setup fade in
        CanvasGroup cg = animCard.GetComponent<CanvasGroup>();
        if (cg == null) cg = animCard.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Set start position
        rect.position = startPos.position;

        // Animate to end position
        float t = 0f;
        Vector3 startPosVec = startPos.position;
        Vector3 endPosVec = endPos.position;

        while (t < 1f)
        {
            if (rect == null || endPos == null) yield break;

            t += Time.deltaTime / drawAnimationDuration;
            rect.position = Vector3.Lerp(startPosVec, endPosVec, t);
            cg.alpha = Mathf.SmoothStep(0f, 1f, t);

            yield return null;
        }

        // Animation complete, destroy temp card
        if (animCard != null) Destroy(animCard);

        // Update actual slot visual
        UpdateSlotVisual(slotIndex);

        Debug.Log($"[ReactionDeckUI] Draw animation complete for {card.cardName}");
    }

    void HandleReactionCardAdded(CardData card, int slotIndex)
    {
        Debug.Log($"[ReactionDeckUI] Card added to slot {slotIndex}: {card.cardName}");

        // Jika slot visual belum ada, create
        while (reactionSlotVisuals.Count <= slotIndex)
        {
            CreateReactionSlotVisual(reactionSlotVisuals.Count);
        }

        UpdateSlotVisual(slotIndex);
    }

    void HandleReactionCardReplaced(CardData oldCard, CardData newCard, int slotIndex)
    {
        Debug.Log($"[ReactionDeckUI] Replaced {oldCard?.cardName} with {newCard.cardName} in slot {slotIndex}");
        UpdateSlotVisual(slotIndex);

        // Hide replace panel
        if (replaceCardPanel != null)
            replaceCardPanel.SetActive(false);
    }

    void HandleReactionTriggered(CardData card, int slotIndex)
    {
        Debug.Log($"[ReactionDeckUI] Reaction triggered: {card.cardName} from slot {slotIndex}");
        StartCoroutine(PlayTriggerAnimation(slotIndex));
    }

    void UpdateSlotVisual(int slotIndex)
    {
        if (slotIndex >= reactionSlotVisuals.Count) return;

        var slots = reactionDeck.GetReactionSlots();
        if (slotIndex >= slots.Count) return;

        var slot = slots[slotIndex];
        GameObject slotObj = reactionSlotVisuals[slotIndex];

        Image cardImage = slotObj.transform.Find("CardImage")?.GetComponent<Image>();

        if (cardImage != null)
        {
            if (slot.cardData != null)
            {
                cardImage.sprite = slot.cardData.icon;
                cardImage.enabled = true;
            }
            else
            {
                cardImage.enabled = false;
            }
        }
    }

    IEnumerator PlayTriggerAnimation(int slotIndex)
    {
        if (slotIndex >= reactionSlotVisuals.Count) yield break;

        GameObject slotObj = reactionSlotVisuals[slotIndex];
        Image cardImage = slotObj.transform.Find("CardImage")?.GetComponent<Image>();

        // Flash effect
        if (cardImage != null)
        {
            Color originalColor = cardImage.color;
            cardImage.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            cardImage.color = originalColor;
        }
    }

    void Update()
    {
        UpdateCooldownVisuals();
    }

    void UpdateCooldownVisuals()
    {
        var slots = reactionDeck.GetReactionSlots();

        for (int i = 0; i < Mathf.Min(slots.Count, reactionSlotVisuals.Count); i++)
        {
            var slot = slots[i];
            GameObject slotObj = reactionSlotVisuals[i];

            Image cooldownOverlay = slotObj.transform.Find("CooldownOverlay")?.GetComponent<Image>();
            Text cooldownText = slotObj.transform.Find("CooldownText")?.GetComponent<Text>();

            if (slot.cardData != null && slot.isOnCooldown)
            {
                float cooldownPercentage = slot.cooldownTimer / slot.cardData.cooldown;

                if (cooldownOverlay != null)
                    cooldownOverlay.fillAmount = cooldownPercentage;

                if (cooldownText != null)
                    cooldownText.text = Mathf.CeilToInt(slot.cooldownTimer).ToString();
            }
            else
            {
                if (cooldownOverlay != null)
                    cooldownOverlay.fillAmount = 0;

                if (cooldownText != null)
                    cooldownText.text = "";
            }
        }
    }

    // Method untuk show replace card panel (dipanggil dari luar)
    public void ShowReplaceCardPanel(CardData newCard)
    {
        pendingReplacementCard = newCard;

        if (replaceCardPanel != null)
        {
            replaceCardPanel.SetActive(true);

            if (replaceCardText != null)
                replaceCardText.text = $"Replace a card with:\n{newCard.cardName}";

            // Setup replace buttons
            var slots = reactionDeck.GetReactionSlots();
            for (int i = 0; i < replaceSlotButtons.Length; i++)
            {
                if (i < slots.Count && slots[i].cardData != null)
                {
                    replaceSlotButtons[i].gameObject.SetActive(true);
                    int slotIndex = i; // Capture untuk lambda
                    replaceSlotButtons[i].onClick.RemoveAllListeners();
                    replaceSlotButtons[i].onClick.AddListener(() => OnReplaceSlotClicked(slotIndex));

                    // Update button text
                    Text btnText = replaceSlotButtons[i].GetComponentInChildren<Text>();
                    if (btnText != null)
                        btnText.text = slots[i].cardData.cardName;
                }
                else
                {
                    replaceSlotButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void OnReplaceSlotClicked(int slotIndex)
    {
        if (pendingReplacementCard != null)
        {
            reactionDeck.ReplaceReactionCard(slotIndex, pendingReplacementCard);
            pendingReplacementCard = null;
        }
    }

    public void CancelReplacement()
    {
        pendingReplacementCard = null;
        if (replaceCardPanel != null)
            replaceCardPanel.SetActive(false);
    }
}