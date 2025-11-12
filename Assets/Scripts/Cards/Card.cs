using UnityEngine;

public enum CardType
{
    Tower,
    Spell,
    Trap,
    Upgrade
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class Card : ScriptableObject
{
    [Header("General Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite artwork;
    public CardType type;
    public int cost;

    [Header("Tower Data")]
    public GameObject towerPrefab; // Only if CardType == Tower

    [Header("Spell Data")]
    public string spellEffectId; // Identifier for script lookup later
}