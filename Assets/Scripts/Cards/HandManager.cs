using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Hand Rules")]
    [SerializeField] private int maxHandSize = 5;

    [Header("UI")]
    [SerializeField] private Transform handUIParent;
    [SerializeField] private GameObject cardUIPrefab;

    private Deck deck;
    private List<Card> hand;
    private readonly List<GameObject> spawnedCardUIs = new();

    public List<Card> CurrentHand => hand;

    public delegate void HandChanged(List<Card> currentHand);
    public event HandChanged OnHandChanged;

    void Awake()
    {
        hand = new List<Card>();
    }

    public void Initialize(Deck sourceDeck)
    {
        deck = sourceDeck;
        RefreshHandUI();
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

        RefreshHandUI();
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

        RefreshHandUI();
        OnHandChanged?.Invoke(hand);
    }

    public void PlayCard(Card card)
    {
        if (!hand.Contains(card)) return;

        hand.Remove(card);
        deck.Discard(card);

        RefreshHandUI();
        OnHandChanged?.Invoke(hand);
    }

    // ---------------------------------------
    // UI REFRESH
    // ---------------------------------------
    private void RefreshHandUI()
    {
        // destroy old UIs
        foreach (var ui in spawnedCardUIs)
            Destroy(ui);

        spawnedCardUIs.Clear();

        // spawn new UIs
        foreach (var card in hand)
        {
            var ui = Instantiate(cardUIPrefab, handUIParent);
            ui.GetComponent<CardUIController>().SetData(card, this);
            spawnedCardUIs.Add(ui);
        }
    }
}
