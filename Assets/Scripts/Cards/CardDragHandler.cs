using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Canvas canvas;
    private RectTransform rect;
    private Card card;
    private HandManager handManager;

    public void Setup(Card data, HandManager manager)
    {
        card = data;
        handManager = manager;

        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        originalParent = transform.parent;
        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPos
        );

        rect.localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // If released above a threshold = consider "played"
        if (eventData.position.y > Screen.height * 0.6f)
        {
            handManager.PlayCard(card);
            return;
        }

        // Otherwise return to hand
        transform.SetParent(originalParent, false);
        rect.anchoredPosition = Vector2.zero;
    }
}
