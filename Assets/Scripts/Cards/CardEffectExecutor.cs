using UnityEngine;

public static class CardEffectExecutor
{
    public static void Execute(Card card, HandManager hand, ResourceManager resources)
    {
        if (card == null) return;

        switch (card.effectType)
        {
            case CardEffectType.DealDamage:
                DealDamage(card.effectValue);
                break;

            case CardEffectType.GainMana:
                GainMana(resources, card.effectValue);
                break;

            case CardEffectType.DrawCards:
                DrawCards(hand, card.effectValue);
                break;

            case CardEffectType.SpawnObject:
                SpawnObject(card);
                break;

            case CardEffectType.Custom:
                ExecuteCustom(card, hand, resources);
                break;

            default:
                break;
        }
    }

    // ----------------------------------------------------
    // Standard Effects
    // ----------------------------------------------------

    private static void DealDamage(int amount)
    {
        Debug.Log($"[Effect] DealDamage: {amount} (Hook enemy system here)");
    }

    private static void GainMana(ResourceManager rm, int amount)
    {
        if (rm == null) return;
        rm.AddMana(amount);
    }

    private static void DrawCards(HandManager hand, int count)
    {
        if (hand == null) return;

        for (int i = 0; i < count; i++)
            hand.DrawCard();
    }

    private static void SpawnObject(Card card)
    {
        if (card.spawnPrefab == null)
        {
            Debug.LogWarning($"[Effect] {card.cardName} has no spawn prefab.");
            return;
        }

        // Replace with tower/spell placement later
        Object.Instantiate(card.spawnPrefab, Vector3.zero, Quaternion.identity);
    }

    // ----------------------------------------------------
    // Custom Effects
    // ----------------------------------------------------

    private static void ExecuteCustom(Card card, HandManager hand, ResourceManager resources)
    {
        if (card.customEffect == null)
        {
            Debug.LogWarning($"[Effect] Custom card '{card.cardName}' has no customEffect assigned.");
            return;
        }

        card.customEffect.Execute(card, hand, resources);
    }
}
