using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteractable : MonoBehaviour, IPointerClickHandler
{
    private Card cardData;
    private HandManager handManager;

    public void Setup(Card data, HandManager manager)
    {
        cardData = data;
        handManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked card: " + cardData.cardName);

        // Later this becomes "play card"
        handManager.PlayCard(cardData);
    }
}
