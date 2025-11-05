using UnityEngine;

public class PlayerEconom : MonoBehaviour
{
    [Header("Player Settings")]
    public int startingGold = 100;
    public int currentGold;

    private void Start()
    {
        currentGold = startingGold;
        Debug.Log("Starting Gold: " + currentGold);
    }

    /// <summary>
    /// Attempt to buy a tower. Returns true if successful.
    /// </summary>
    public bool BuyTower(int towerPrice)
    {
        if (currentGold >= towerPrice)
        {
            currentGold -= towerPrice;
            Debug.Log("Bought tower for " + towerPrice + " gold. Remaining gold: " + currentGold);
            return true;
        }
        else
        {
            Debug.Log("Not enough gold to buy tower. You have: " + currentGold + ", need: " + towerPrice);
            return false;
        }
    }

    /// <summary>
    /// Sell a tower and get gold back. Optionally, apply a refund multiplier (like 50%).
    /// </summary>
    public void SellTower(int towerPrice, float refundMultiplier = 0.5f)
    {
        int refundAmount = Mathf.RoundToInt(towerPrice * refundMultiplier);
        currentGold += refundAmount;
        Debug.Log("Sold tower for " + refundAmount + " gold. Current gold: " + currentGold);
    }

    /// <summary>
    /// Add gold manually (for rewards, income, etc.)
    /// </summary>
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log("Added " + amount + " gold. Current gold: " + currentGold);
    }
}