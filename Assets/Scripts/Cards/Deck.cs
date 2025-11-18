using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public List<Card> allCards = new List<Card>();
    private List<Card> drawPile = new List<Card>();
    private List<Card> discardPile = new List<Card>(); 


public List<Card> GetDrawPile() => drawPile != null ? new List<Card>(drawPile) : new List<Card>();
public List<Card> GetDiscardPile() => discardPile ?? new List<Card>();


    public void Initialize()
    {
        drawPile = new List<Card>(allCards);
        Shuffle(drawPile);
    }

    public Card DrawCard()
    {
        if (drawPile.Count == 0)
            RefillFromDiscard();

        if (drawPile.Count == 0)
            return null;

        Card card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    public void Discard(Card card)
    {
        if (card != null)
            discardPile.Add(card);
    }

    private void RefillFromDiscard()
    {
        drawPile = new List<Card>(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public int DrawCount => drawPile.Count;
    public int DiscardCount => discardPile.Count;
}