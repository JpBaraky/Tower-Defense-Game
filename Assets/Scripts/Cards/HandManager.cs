using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Hand Rules")]
    [SerializeField] private int maxHandSize = 5;

    [Header("UI")]
    [SerializeField] private Transform handUIParent;
    [SerializeField] private GameObject cardUIPrefab;
    [HideInInspector]
    public Deck deck;
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

    // External API used by CardInteractable/drag handler
public bool RequestPlayCard(Card card)
{
    if (card == null || !hand.Contains(card)) 
        return false;

    if (ResourceManager.Instance == null)
    {
        PlayCard(card);
        return true;
    }

    if (!ResourceManager.Instance.CanAfford(card.cost))
    {
        Debug.Log("Cannot play card, not enough mana: " + card.cardName);
       // RefreshHandUI();
        return false;
        
    }

    bool spent = ResourceManager.Instance.Spend(card.cost);
    if (!spent) 
        return false;

    PlayCard(card);
    return true;
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

    // internal execution after validation
private void PlayCard(Card card)
{
    if (!hand.Contains(card)) return;

    hand.Remove(card);
    deck.Discard(card);

    CardEffectExecutor.Execute(
        card,
        this,                       // hand manager
        ResourceManager.Instance    // your real resource manager
    );

    RefreshHandUI();
    OnHandChanged?.Invoke(hand);
}


    // ---------------------------------------
    // UI REFRESH
    // ---------------------------------------
    public void RefreshHandUI()
    {
        // destroy old UIs
        foreach (var ui in spawnedCardUIs)
            Destroy(ui);

        spawnedCardUIs.Clear();

        // spawn new UIs
        foreach (var card in hand)
        {
            var uiObj = Instantiate(cardUIPrefab, handUIParent);
            var controller = uiObj.GetComponent<CardUIController>();
            controller.SetData(card, this); // controller will also hook up drag/click handlers
            spawnedCardUIs.Add(uiObj);
        }
    }

    // helper for CardUI to query affordability (used on UI updates)
    public bool CanPlay(Card card)
    {
        if (card == null) return false;
        if (ResourceManager.Instance == null) return true; // if no RM, allow
        return ResourceManager.Instance.CanAfford(card.cost);
    }
   
}
