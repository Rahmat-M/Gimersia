using UnityEngine;

public class GameManager : MonoBehaviour {
    [Header("Systems")]
    public DeckSystem deck;
    public PlayerCombat playerCombat;
    public PlayerHealth playerHealth;
    public ReactionDeckSystem reactionDeck;
    public ActiveDeckSystem activeDeck;

    //debug
    public CardData testReactionCard;

    void Start() {
        deck.OnCardPlayed += HandleCardPlayed;
    }

    void HandleCardPlayed(CardInstance card) {
        Debug.Log("[GameManager] Card played: " + card.data.cardName);
        // delegasikan ke PlayerCombat untuk mengeksekusi efek kartu
        playerCombat.PlayCard(card);
    }

    //debugging only
    void Update() {
        // Press 'H' untuk test add reaction card
        if (Input.GetKeyDown(KeyCode.H)) {
            TestAddReactionCard(testReactionCard); // Assign di inspector
        }

        // Press 'J' untuk test take damage
        if (Input.GetKeyDown(KeyCode.J)) {
            TestTakeDamage(20f);
        }

        // Press 'K' untuk test heal
        if (Input.GetKeyDown(KeyCode.K)) {
            TestHeal(15f);
        }
    }

    // Method untuk testing: Add reaction card (bisa dipanggil dari UI button atau event)
    public void TestAddReactionCard(CardData reactionCard) {
        if (reactionDeck != null) {
            // Jika semua slot penuh, show replace panel
            if (!reactionDeck.HasEmptySlot() && reactionDeck.GetReactionSlots().Count >= reactionDeck.maxReactionSlots) {
                var reactionUI = FindFirstObjectByType<ReactionDeckUI>();
                if (reactionUI != null) {
                    reactionUI.ShowReplaceCardPanel(reactionCard);
                }
            } else {
                reactionDeck.AddReactionCard(reactionCard);
            }
        }
    }

    // Method untuk testing: Replace active card (untuk upgrade system)
    public void TestReplaceActiveCard(CardData newActiveCard) {
        if (activeDeck != null) {
            activeDeck.ReplaceActiveCard(newActiveCard);
        }
    }

    // Method untuk testing: Simulate damage (bisa dipanggil dari UI button)
    public void TestTakeDamage(float damage) {
        if (playerHealth != null) {
            playerHealth.TakeDamage(damage);
        }
    }

    // Method untuk testing: Simulate heal (bisa dipanggil dari UI button)
    public void TestHeal(float amount) {
        if (playerHealth != null) {
            playerHealth.Heal(amount);
        }
    }
}