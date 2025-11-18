using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HandManager handManager;

    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private TMP_Text discardCountText;

    private bool isHovering = false;

    private void Start()
    {
        handManager = FindAnyObjectByType<HandManager>();

        // Hide at start
        if(deckCountText != null)
            deckCountText.enabled = false;
        if(discardCountText != null)
            discardCountText.enabled = false;
    }

    private void Update()
    {
        if (!isHovering) return;
        if (handManager == null || handManager.deck == null) return;
        if(deckCountText != null)
            deckCountText.text = handManager.deck.DrawCount.ToString();
        if(discardCountText != null)
            discardCountText.text = handManager.deck.DiscardCount.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if(deckCountText != null)
        deckCountText.enabled = true;
        if(discardCountText != null)
        discardCountText.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if(deckCountText != null)
        deckCountText.enabled = false;
        if(discardCountText != null)
        discardCountText.enabled = false;
    }
}