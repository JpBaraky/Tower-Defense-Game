using UnityEngine;

[CreateAssetMenu(menuName = "Card/Custom Effects/Double Mana")]
public class DoubleManaEffect : CardCustomEffect
{
    public override void Execute(Card card, HandManager hand, ResourceManager resources)
    {
        Debug.Log($"[Custom Effect] {card.cardName} activated: Double mana");

        if (resources == null) return; 
        resources.AddMana(resources.CurrentMana);
    }

   
}