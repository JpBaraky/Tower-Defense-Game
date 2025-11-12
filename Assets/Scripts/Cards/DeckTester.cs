using UnityEngine;
using System.Collections.Generic;

public class DeckTester : MonoBehaviour
{
    [SerializeField] private List<Card> startingCards; // shows in Inspector
    private Deck deck;

    void Start()
    {
        deck = new Deck { allCards = startingCards };
        deck.Initialize();

        for (int i = 0; i < 5; i++)
        {
            var card = deck.DrawCard();
            if (card != null)
                Debug.Log("Drew: " + card.cardName);
        }
    }
}
