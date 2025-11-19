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

            case CardEffectType.SpawnTower:
                SpawnTower(card);
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
 Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == null) continue;

            e.TakeDamage(amount);

            Renderer r = e.GetComponentInChildren<Renderer>();
            if (r != null)
                e.StartCoroutine(FlashMaterial(r, 0.1f));
        }
    }

    private static System.Collections.IEnumerator FlashMaterial(Renderer r, float duration)
    {
        Material mat = r.material; // uses instance
        Color original = mat.color;

        mat.color = Color.red;
        yield return new WaitForSeconds(duration);
        mat.color = original;
    }

    private static System.Collections.IEnumerator Flash(SpriteRenderer sr, float duration)
    {
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(duration);
        sr.color = original;
    
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

        
        Object.Instantiate(card.spawnPrefab, Vector3.zero, Quaternion.identity);
    }
    private static void SpawnTower(Card card)
    {
        if (card.spawnPrefab == null)
        {
            Debug.LogWarning($"[Effect] {card.cardName} has no spawn prefab.");
            return;
        }

         TowerPlacement towerPlacement;
         towerPlacement = TowerPlacement.Instance; 
         towerPlacement.towerPrefab = card.spawnPrefab;
      
        towerPlacement.CanPlaceTower();
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
