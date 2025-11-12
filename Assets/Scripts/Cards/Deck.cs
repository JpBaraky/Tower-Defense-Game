using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Deck
{
    public List<Card> allCards;
    private Queue<Card> drawPile;
    private List<Card> discardPile;

    public void Initialize()
    {
        if (allCards == null)
            allCards = new List<Card>();

        var shuffled = allCards.OrderBy(x => Random.value).ToList();
        drawPile = new Queue<Card>(shuffled);
        discardPile = new List<Card>();
    }

    public Card DrawCard()
    {
        if (drawPile == null) Initialize();

        if (drawPile.Count == 0)
            Reshuffle();

        if (drawPile.Count == 0)
            return null;

        return drawPile.Dequeue();
    }

    public void Discard(Card card)
    {
        discardPile.Add(card);
    }

    private void Reshuffle()
    {
        if (discardPile.Count == 0)
            return;

        discardPile = discardPile.OrderBy(x => Random.value).ToList();
        foreach (var card in discardPile)
            drawPile.Enqueue(card);

        discardPile.Clear();
    }

    public int Remaining => drawPile?.Count ?? 0;
}