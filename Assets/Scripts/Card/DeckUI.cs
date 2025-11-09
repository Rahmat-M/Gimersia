using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour
{
    public DeckSystem deck;
    // public Image[] handSlotImages; // Comment out atau disable di Inspector
    // public Text[] handSlotTexts; // Comment out atau disable di Inspector
    public Button playNextButton;
    [Header("Card Slot Positions")]
    public Transform pileSlot;
    public Transform[] handSlots;
    public Transform discardSlot;
    public GameObject cardUIPrefab;
    private Dictionary<CardInstance, GameObject> cardVisuals = new Dictionary<CardInstance, GameObject>();

    void Start()
    {
        deck.OnCardDrawn += HandleCardDrawn;
        deck.OnCardDiscarded += HandleCardDiscarded;
        deck.OnCardPlayed += HandleCardPlayed;
        if (playNextButton != null)
            playNextButton.onClick.AddListener(OnPlayNextClicked);
        // Disable static slots jika ada
        // foreach (var img in handSlotImages) if (img) img.gameObject.SetActive(false);
        // foreach (var txt in handSlotTexts) if (txt) txt.gameObject.SetActive(false);
    }

    void HandleCardDrawn(CardInstance card, int handIndex)
    {
        Debug.Log($"[DeckUI] Animating draw {card.data.cardName} to hand[{handIndex}]");
        GameObject uiCard = Instantiate(cardUIPrefab, transform);
        uiCard.name = $"CardUI_{card.data.cardName}";
        Image img = uiCard.GetComponentInChildren<Image>();
        if (img != null) img.sprite = card.data.icon;
        cardVisuals[card] = uiCard;
        StartCoroutine(AnimateCardMove(uiCard, pileSlot, handSlots[handIndex], 0.6f, true));
    }

    // Diubah: Hanya highlight, tanpa move/destroy
    void HandleCardPlayed(CardInstance card)
    {
        if (cardVisuals.TryGetValue(card, out GameObject uiCard))
        {
            Debug.Log($"[DeckUI] Highlighting {card.data.cardName}");
            StartCoroutine(Highlight(uiCard));
        }
    }

    private IEnumerator Highlight(GameObject uiCard)
    {
        Image img = uiCard.GetComponentInChildren<Image>();
        if (img != null) img.color = Color.yellow;
        yield return new WaitForSeconds(0.3f);
        if (img != null) img.color = Color.white;
    }

    void HandleCardDiscarded(CardInstance card)
    {
        if (cardVisuals.TryGetValue(card, out GameObject uiCard))
        {
            Debug.Log($"[DeckUI] Direct discard anim {card.data.cardName}");
            StartCoroutine(DiscardAndDestroy(uiCard, discardSlot, 0.5f));
            cardVisuals.Remove(card);
        }
    }

    // Fixed: Gunakan anchoredPosition untuk UI stability
    public IEnumerator AnimateCardMove(GameObject cardObj, Transform start, Transform end, float duration, bool fadeIn = true)
    {
        if (cardObj == null || start == null || end == null) yield break;
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        if (rect == null) yield break;
        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>() ?? cardObj.AddComponent<CanvasGroup>();
        cg.alpha = fadeIn ? 0f : 1f;
        Vector3 startPos = start.position;
        Vector3 endPos = end.position;
        rect.position = startPos;
        float t = 0f;
        while (t < 1f)
        {
            if (rect == null || end == null) yield break;
            t += Time.deltaTime / duration;
            rect.position = Vector3.Lerp(startPos, endPos, t);
            cg.alpha = fadeIn ? Mathf.SmoothStep(0f, 1f, t) : Mathf.SmoothStep(1f, 0f, t);
            yield return null;
        }
        if (rect != null) rect.position = endPos;
        if (cg != null) cg.alpha = fadeIn ? 1f : 0f;
    }

    private IEnumerator DiscardAndDestroy(GameObject cardObj, Transform discardSlot, float duration)
    {
        yield return AnimateCardMove(cardObj, cardObj.transform, discardSlot, duration, false);
        if (cardObj != null) Destroy(cardObj);
    }

    void OnPlayNextClicked()
    {
        // Ganti ke play full hand jika manual
        StartCoroutine(deck.PlayHandRoutine());
    }

    // HAPUS INI: void Update() { RefreshHandUI(); }
    // HAPUS INI: void RefreshHandUI() { ... } // Tidak perlu lagi, pure animasi
}

