using UnityEngine;
using UnityEngine.InputSystem;

public static class CardEffectExecutor
{
    public static void Execute(Card card, HandManager hand, ResourceManager resources)
    {
        if (card == null) return;

        switch (card.targetType)
        {
            case TargetType.None:
                ExecuteNoTarget(card, hand, resources);
                break;

            case TargetType.All:
                ExecuteAll(card, hand, resources);
                break; 
            case TargetType.Area:
    ExecuteArea(card, hand, resources);
    break; 

            case TargetType.Single:
                ExecuteSingle(card, hand, resources);
                break;
        }
    }

    // ============================================================
    // TARGET: NONE
    // ============================================================

    private static void ExecuteNoTarget(Card card, HandManager hand, ResourceManager resources)
    {
        switch (card.effectType)
        {
            case CardEffectType.Damage:
                DealDamageAll(card.effectValue);   // no target → apply instantly to all
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
        }
    }

    // ============================================================
    // TARGET: ALL
    // ============================================================

    private static void ExecuteAll(Card card, HandManager hand, ResourceManager resources)
    {
        switch (card.effectType)
        {
            case CardEffectType.Damage:
                DealDamageAll(card.effectValue);
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
        }
    }

    // ============================================================
    // TARGET: SINGLE
    // ============================================================

    private static void ExecuteSingle(Card card, HandManager hand, ResourceManager resources)
    {
        Vector3 worldPos = GetCardWorldPosition();

        GameObject anchor = new GameObject("ArrowAnchor");
        anchor.transform.position = worldPos;

        TargetSelector.Instance.BeginSelecting(anchor.transform, (Enemy e) =>
        {
            Object.Destroy(anchor);

            if (e == null) return;

            switch (card.effectType)
            {
                case CardEffectType.Damage:
                    ApplyDamageWithFlash(e, card.effectValue);
                    break;

                case CardEffectType.Custom:
                    // Custom effects handle how they use the selected target
                    card.customEffect?.ExecuteOnTarget(card, e, hand, resources);
                    break;

                case CardEffectType.GainMana:
                case CardEffectType.DrawCards:
                case CardEffectType.SpawnObject:
                case CardEffectType.SpawnTower:
                    Debug.LogWarning($"Effect {card.effectType} does not support Single targeting.");
                    break;
            }
        });
    }

    // ============================================================
    // EFFECT IMPLEMENTATIONS
    // ============================================================

    private static void ApplyDamageWithFlash(Enemy e, float amount)
    {
        e.TakeDamage(amount);

        Renderer r = e.GetComponentInChildren<Renderer>();
        if (r != null)
            e.StartCoroutine(FlashMaterial(r, 0.1f));
    }

    private static void DealDamageAll(int amount)
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

    private static void GainMana(ResourceManager rm, int amount)
    {
        rm?.AddMana(amount);
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

        TowerPlacement tp = TowerPlacement.Instance;
        tp.towerPrefab = card.spawnPrefab;
        tp.CanPlaceTower();
    }

    private static void ExecuteCustom(Card card, HandManager hand, ResourceManager resources)
    {
        card.customEffect?.Execute(card, hand, resources);
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private static Vector3 GetCardWorldPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        return cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 1f));
    }

    private static System.Collections.IEnumerator FlashMaterial(Renderer r, float t)
    {
        Material mat = r.material;
        Color original = mat.color;

        mat.color = Color.red;
        yield return new WaitForSeconds(t);
        mat.color = original;
    }
   private static void ExecuteArea(Card card, HandManager hand, ResourceManager resources)
{
    if (card.areaRadius <= 0f)
    {
        Debug.LogWarning("Card has no valid area radius.");
        return;
    }

    GameObject previewGO = new GameObject("AreaPreview");
    AreaTargetPreview preview = previewGO.AddComponent<AreaTargetPreview>();
    preview.Initialize(card.areaRadius);

    preview.OnConfirm = (Vector3 center) =>
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == null) continue;

            float dist = Vector3.Distance(new Vector3(e.transform.position.x, 0f, e.transform.position.z),
                                         new Vector3(center.x, 0f, center.z));
            if (dist > card.areaRadius) continue;

            switch (card.effectType)
            {
                case CardEffectType.Damage:
                    ApplyDamageWithFlash(e, card.effectValue);
                    break;

                case CardEffectType.Custom:
                    card.customEffect?.ExecuteOnTarget(card, e, hand, resources);
                    break;

                default:
                    Debug.LogWarning($"{card.effectType} does not support Area targeting.");
                    break;
            }
        }
    };
}


}
