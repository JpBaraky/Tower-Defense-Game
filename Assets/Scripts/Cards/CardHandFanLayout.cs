using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteAlways]
public class CardHandFanLayout : MonoBehaviour
{
    [Header("Fan Settings")]
    [SerializeField] private float radius = 450f;
    [SerializeField] private float yOffset = -80f;
    [SerializeField] private float smooth = 0.0f;

    // The arc looks best when 5 cards fill -35° to +35° (total 70°)
    private const float MAX_VISIBLE_ANGLE = 35f;     // per side
    private const int BASE_CARD_COUNT = 5;           // arc calibrated for 5 cards
    [HideInInspector]
    public bool isDragging;

    private void Start(){}
  

    private void Update()
    {
        if(!isDragging){
        ApplyFan();
        }
    }

    private void ApplyFan()
    {
        int count = transform.childCount;
        if (count == 0) return;

        // proportion of arc based on how many cards you have relative to the ideal 5
        float t = Mathf.Clamp01((count - 1) / (float)(BASE_CARD_COUNT - 1));

        // final angle for edges
        float edgeAngle = Mathf.Lerp(0f, MAX_VISIBLE_ANGLE, t);

        float angleStep = count > 1 ? (edgeAngle * 2f) / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = transform.GetChild(i).GetComponent<RectTransform>();
            if (card == null) continue;

            float angle = -edgeAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 targetPos = new Vector2(
                Mathf.Sin(rad) * radius + count * 50f,
                yOffset - Mathf.Cos(rad) * radius + radius
            );

            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

            if (smooth > 0f)
            {
                card.anchoredPosition = Vector2.Lerp(card.anchoredPosition, targetPos, Time.unscaledDeltaTime * smooth);
                card.localRotation = Quaternion.Lerp(card.localRotation, targetRot, Time.unscaledDeltaTime * smooth);
            }
            else
            {
                card.anchoredPosition = targetPos;
                card.localRotation = targetRot;
            }
        }
    }
}
