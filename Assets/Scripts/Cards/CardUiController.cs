using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CardUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    [Header("Affordability visuals")]
    [SerializeField] private Image costBackground;
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.gray;

    private Card cardData;
    private HandManager handManager;

    private CanvasGroup canvasGroup;
    private CardInteractable interactable;
    private CardDragHandler dragHandler;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        interactable = GetComponent<CardInteractable>();
        dragHandler = GetComponent<CardDragHandler>();
    }

    public void SetData(Card data, HandManager manager)
    {
        cardData = data;
        handManager = manager;

        // --- card interaction setup ---
        if (interactable != null)
            interactable.Setup(cardData, handManager);

        if (dragHandler != null)
            dragHandler.Setup(cardData, handManager);

        // --- UI visuals ---
        nameText.text = data.cardName;
        descriptionText.text = data.description;
        costText.text = data.cost.ToString();

        if (data.artwork != null)
            artworkImage.sprite = data.artwork;

        UpdateAffordability();

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnManaChanged += OnManaChanged;
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnManaChanged -= OnManaChanged;
    }

    private void OnManaChanged(int current, int max)
    {
        UpdateAffordability();
    }

    private void UpdateAffordability()
    {
        if (cardData == null) return;

        // check affordability
        bool affordable = handManager != null
            ? handManager.CanPlay(cardData)
            : (ResourceManager.Instance?.CanAfford(cardData.cost) ?? true);

        // visual feedback only
        canvasGroup.alpha = affordable ? 1f : 0.55f;

        // IMPORTANT:
        // Do NOT block raycasts, otherwise drag stops working.
        // Click logic is handled inside CardInteractable anyway.
        canvasGroup.blocksRaycasts = true;

        if (costBackground != null)
            costBackground.color = affordable ? affordableColor : unaffordableColor;
    }

    public Card GetCardData() => cardData;
}
