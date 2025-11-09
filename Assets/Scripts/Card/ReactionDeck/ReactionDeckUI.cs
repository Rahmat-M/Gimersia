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
            reactionDeck.OnReactionCardAdded += HandleReactionCardAdded;
            reactionDeck.OnReactionCardReplaced += HandleReactionCardReplaced;
            reactionDeck.OnReactionTriggered += HandleReactionTriggered;
        }

        InitializeReactionSlots();

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
    }

    void CreateReactionSlotVisual(int index)
    {
        GameObject slotObj = Instantiate(reactionSlotPrefab, reactionSlotsContainer);
        slotObj.name = $"ReactionSlot_{index}";

        // Setup slot visual
        Image cardImage = slotObj.transform.Find("CardImage")?.GetComponent<Image>();
        Image cooldownOverlay = slotObj.transform.Find("CooldownOverlay")?.GetComponent<Image>();
        Text cooldownText = slotObj.transform.Find("CooldownText")?.GetComponent<Text>();

        if (cardImage != null) cardImage.enabled = false;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
        if (cooldownText != null) cooldownText.text = "";

        reactionSlotVisuals.Add(slotObj);
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