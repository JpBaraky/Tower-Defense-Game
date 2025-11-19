public interface ICardCustomEffect
{
     void Execute(Card card, HandManager hand, ResourceManager resources);
    void ExecuteOnTarget(Card card, Enemy target, HandManager hand, ResourceManager resources);
}