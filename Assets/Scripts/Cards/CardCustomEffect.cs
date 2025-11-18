using UnityEngine;

public abstract class CardCustomEffect : ScriptableObject
{
    public abstract void Execute(Card card, HandManager hand, ResourceManager resources);
}