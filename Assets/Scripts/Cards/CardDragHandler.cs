using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Canvas canvas;
    private RectTransform rect;
    private CanvasGroup cg;

    private Vector2 originalAnchoredPos;
    private Coroutine returnRoutine;

    [SerializeField] private float returnDuration = 0.25f;

    private Card card;
    private HandManager handManager;
   
    private CardHandFanLayout cardHandFanLayout;
    private CardUIController cardUIController;
 

    void Awake()
    {
        cardHandFanLayout = FindAnyObjectByType<CardHandFanLayout>();
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
        cardUIController = eventData.pointerPress.GetComponent<CardUIController>();
        cardHandFanLayout.isDragging = true;
        if (canvas == null) return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        originalParent = transform.parent;
        originalAnchoredPos = rect.anchoredPosition;

        cg.blocksRaycasts = false;
        cg.alpha = 0.75f;

        //transform.SetParent(canvas.transform, true);
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

        rect.localPosition = localPos + new Vector2(0f, 100f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        cg.blocksRaycasts = true;
        cg.alpha = 1f;

        bool played = false;

        if (card != null && handManager != null)
        {
            float pointerY = eventData.position.y;
            float threshold = Screen.height * 0.6f;

            if (pointerY > threshold)
                played = handManager.RequestPlayCard(card);
                cardHandFanLayout.isDragging = false;
        }

        if (!played)
        {
            Debug.Log("Returning card to hand: " + card.cardName);
            transform.SetParent(originalParent, false);
            cardUIController.UpdateAffordability();
            StartReturnAnimation();
           
        }
    }

    private void StartReturnAnimation()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToOriginalPos());
    }

    private IEnumerator ReturnToOriginalPos()
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end = originalAnchoredPos;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / returnDuration;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        rect.anchoredPosition = end;
        returnRoutine = null;
      
        cardHandFanLayout.isDragging = false;
    }
}
