using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HandTester : MonoBehaviour
{
    [SerializeField] private List<Card> startingCards;
    private Deck deck;
    public HandManager handManager;

    void Start()
    {
        deck = new Deck();
        deck.allCards = new List<Card>(startingCards);
        deck.Initialize();

        handManager = GetComponent<HandManager>();
        handManager.Initialize(deck);

        handManager.OnHandChanged += cards =>
        {
          
            foreach (var c in cards) Debug.Log(" - " + c.cardName);
        };
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            handManager.DrawCard();
        }

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            if (handManager.CurrentHand.Count > 0)
            {
                var card = handManager.CurrentHand[0];
                handManager.DiscardCard(card);
               
            }
            else
            {
                Debug.Log("No cards to discard.");
            }
        }
    }
}