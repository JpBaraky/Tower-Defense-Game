using UnityEngine;

public abstract class CardCustomEffect : ScriptableObject
{
            public abstract void Execute(Card card, HandManager hand, ResourceManager resources);
            public abstract void ExecuteOnTarget(Card card, Enemy target, HandManager hand, ResourceManager resources); 
    }