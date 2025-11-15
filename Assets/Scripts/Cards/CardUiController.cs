using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    private Card cardData;

    public void SetCard(Card data)
    {
        cardData = data;

        nameText.text = data.cardName;
        descriptionText.text = data.description;
        costText.text = data.cost.ToString();
        if(data.artwork != null)
        artworkImage.sprite = data.artwork;
    }

    public Card GetCardData() => cardData;
}