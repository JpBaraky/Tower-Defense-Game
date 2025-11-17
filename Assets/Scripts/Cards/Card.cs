using UnityEngine;

public enum CardEffectType
{
    None,
    DealDamage,
    SpawnObject,
    DrawCards,
    GainMana,
    Custom
}

[CreateAssetMenu(menuName = "Card/New Card")]
public class Card : ScriptableObject
{
    public string cardName;
    public string description;
    public int cost;
    public Sprite artwork;

    public CardEffectType effectType;
    public int effectValue;
    public GameObject spawnPrefab;
    public CardCustomEffect customEffect;

}
