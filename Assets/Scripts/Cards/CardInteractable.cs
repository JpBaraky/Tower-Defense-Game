using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteractable : MonoBehaviour, IPointerClickHandler
{
    private Card card;
    private HandManager handManager;

    public void Setup(Card cardData, HandManager manager)
    {
        card = cardData;
        handManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (card == null || handManager == null) return;

        // Left click = play the card
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            handManager.PlayCard(card);
        }

        // Right click = discard
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            handManager.DiscardCard(card);
        }
    }
}
