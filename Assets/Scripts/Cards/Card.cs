using UnityEngine;

public enum CardEffectType
{
    Damage,
    GainMana,
    DrawCards,
    SpawnObject,
    SpawnTower,
    Custom
}
public enum TargetType
{
    None,
    Single,
   
    All,
     Area
}

[CreateAssetMenu(menuName = "Card/New Card")]
public class Card : ScriptableObject
{
    public string cardName;
    [TextArea]
    public string description;
    public int cost;
    public Sprite artwork;
    public TargetType targetType;
    public float areaRadius;


    public CardEffectType effectType;
    public int effectValue;
    public GameObject spawnPrefab;
    public CardCustomEffect customEffect;
    #if UNITY_EDITOR
    private void OnValidate()
    {
        cardName = name;
    }
#endif

}
