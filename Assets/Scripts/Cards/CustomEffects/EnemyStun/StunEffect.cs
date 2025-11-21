using UnityEngine;

[CreateAssetMenu(menuName = "Card/Custom Effects/Stun Enemy")]
public class StunEffect : CardCustomEffect
{
    [SerializeField] private float stunDuration = 2f;

    public override void Execute(Card card, HandManager hand, ResourceManager resources)
    {
        // No-target version does nothing
    }

    public override void ExecuteOnTarget(Card card, Enemy target, HandManager hand, ResourceManager resources)
    {
        if (target == null) return;

        stunDuration = card.effectValue;
        StunRuntime stun = target.GetComponent<StunRuntime>();
        if (stun == null)
            stun = target.gameObject.AddComponent<StunRuntime>();

        stun.StartStun(stunDuration);
    }
}