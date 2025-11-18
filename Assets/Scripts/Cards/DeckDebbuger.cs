using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class DeckDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandManager handManager;

    [Header("Debug Info (Read-Only)")]
    [SerializeField, TextArea(2, 10)] private string drawPileContents;
    [SerializeField, TextArea(2, 10)] private string discardPileContents;
    [SerializeField, TextArea(2, 10)] private string handContents;

    void Update()
    {
        if (handManager == null)
        {
            drawPileContents = discardPileContents = handContents = "(no HandManager assigned)";
            return;
        }

        var deckField = typeof(HandManager).GetField("deck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (deckField == null)
        {
            drawPileContents = discardPileContents = handContents = "(deck not found)";
            return;
        }

        Deck deck = (Deck)deckField.GetValue(handManager);
        if (deck == null)
        {
            drawPileContents = discardPileContents = handContents = "(no deck instance)";
            return;
        }

        drawPileContents = GetCardList(deck.GetDrawPile());
        discardPileContents = GetCardList(deck.GetDiscardPile());
        handContents = GetCardList(handManager.CurrentHand);
    }

    private string GetCardList(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return "(empty)";

        string list = "";
        foreach (var c in cards)
            list += c.cardName + "\n";
        return list;
    }
}
