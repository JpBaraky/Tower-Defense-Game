using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Canvas canvas;
    private RectTransform rect;
    private CanvasGroup cg;

    private Card card;
    private HandManager handManager;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(Card data, HandManager manager)
    {
        card = data;
        handManager = manager;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        originalParent = transform.parent;

        cg.blocksRaycasts = false;
        cg.alpha = 0.75f;

        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPos
        );

        rect.localPosition = localPos;
    }

public void OnEndDrag(PointerEventData eventData)
{
    cg.blocksRaycasts = true;
    cg.alpha = 1f;

    bool played = false;

    if (card != null && handManager != null)
    {
        float pointerY = eventData.position.y;
        float threshold = Screen.height * 0.4f;

        if (pointerY > threshold)
        {
            played = handManager.RequestPlayCard(card);
        }
    }

    if (!played)
    {
        // return to hand
        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            rect.anchoredPosition = Vector2.zero;
        }
    }
}

}
