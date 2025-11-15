using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Card card;
    private HandManager manager;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private Transform originalParent;
    private Vector3 originalPosition;

    public void Setup(Card data, HandManager handManager)
    {
        card = data;
        manager = handManager;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup == null) return;

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;

        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        rectTransform.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        bool played = TryPlayCard(eventData);

        if (!played)
        {
            // return to original place
            rectTransform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private bool TryPlayCard(PointerEventData eventData)
    {
        if (card == null || manager == null) return false;

        // optional: check drop zone tag or layer
        // here we simply auto-play when dragging up
        if (eventData.position.y > Screen.height * 0.7f)
        {
            manager.RequestPlayCard(card);
            return true;
        }

        return false;
    }
}
