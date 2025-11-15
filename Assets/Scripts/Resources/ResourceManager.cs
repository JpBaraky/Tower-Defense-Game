using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private int maxMana = 10;
    [SerializeField] private int startMana = 3;

    public int CurrentMana { get; private set; }
    public int MaxMana => maxMana;

    // (current, max)
    public event Action<int,int> OnManaChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentMana = Mathf.Clamp(startMana, 0, maxMana);
    }

    void Start()
    {
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    public bool CanAfford(int cost) => cost <= CurrentMana;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        CurrentMana -= cost;
        OnManaChanged?.Invoke(CurrentMana, maxMana);
        return true;
    }

    public void AddMana(int amount)
    {
        CurrentMana = Mathf.Clamp(CurrentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    public void SetMana(int value)
    {
        CurrentMana = Mathf.Clamp(value, 0, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    // optional refill to max (useful between waves)
    public void RefillToMax()
    {
        CurrentMana = maxMana;
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }
}
