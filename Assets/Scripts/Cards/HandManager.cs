using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [SerializeField] private int maxHandSize = 5;
    private Deck deck;
    private List<Card> hand;

    public delegate void HandChanged(List<Card> currentHand);
    public event HandChanged OnHandChanged;

    public List<Card> CurrentHand => hand;

    void Awake()
    {
        hand = new List<Card>();
    }

    public void Initialize(Deck sourceDeck)
    {
        deck = sourceDeck;
        // start empty — no opening draw
        OnHandChanged?.Invoke(hand);
    }

    public void DrawCard()
    {
        if (hand.Count >= maxHandSize)
        {
            Debug.Log("Hand is full, cannot draw more cards.");
            return;
        }

        Card drawn = deck.DrawCard();
        if (drawn == null)
        {
            Debug.Log("No more cards in deck.");
            return;
        }

        hand.Add(drawn);
        OnHandChanged?.Invoke(hand);
    }

    public void DiscardCard(Card card)
    {
        if (card == null || !hand.Contains(card))
        {
            Debug.Log("Card not found in hand to discard.");
            return;
        }

        hand.Remove(card);
        deck.Discard(card);
        OnHandChanged?.Invoke(hand);
    }

    public void PlayCard(Card card)
    {
        if (!hand.Contains(card)) return;
        hand.Remove(card);
        deck.Discard(card);
        OnHandChanged?.Invoke(hand);
    }
}
