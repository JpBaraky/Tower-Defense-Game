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
    [SerializeField] private Image costBackground; // optional colored background you can tint
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.gray;

    private Card cardData;
    private HandManager handManager;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetData(Card data, HandManager manager)
    {
        cardData = data;
        handManager = manager;

        nameText.text = data.cardName;
        descriptionText.text = data.description;
        costText.text = data.cost.ToString();
        if (data.artwork != null) artworkImage.sprite = data.artwork;

        UpdateAffordability();

        // subscribe to mana changes so UI updates automatically
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnManaChanged += OnManaChanged;
    }

    private void OnDestroy()
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
        bool affordable = true;
        if (cardData != null)
        {
            affordable = handManager != null ? handManager.CanPlay(cardData) : (ResourceManager.Instance?.CanAfford(cardData.cost) ?? true);
        }

        // visual treatment: desaturate/alpha and tint cost
        canvasGroup.alpha = affordable ? 1f : 0.6f;
        canvasGroup.blocksRaycasts = affordable; // prevent clicks if unaffordable

        if (costBackground != null)
            costBackground.color = affordable ? affordableColor : unaffordableColor;
    }

    public Card GetCardData() => cardData;
}
