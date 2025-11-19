using UnityEngine;

[CreateAssetMenu(menuName = "Card/Custom Effects/Double Mana")]
public class DoubleManaEffect : CardCustomEffect
{
    // No-target effect
    public override void Execute(Card card, HandManager hand, ResourceManager resources)
    {
        if (resources == null) return;

        int current = resources.CurrentMana;
        resources.AddMana(current);
    }

    // Single-target effect (required by new system)
    // This effect does not use the target, but must still exist
    public override void ExecuteOnTarget(Card card, Enemy target, HandManager hand, ResourceManager resources)
    {
        if (resources == null) return;

        int current = resources.CurrentMana;
        resources.AddMana(current);
    }
}