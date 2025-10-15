using TMPro;
using UnityEngine;

// Castle script
public class CastleHealth : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;
    public TextMeshProUGUI healthText;

    void Awake()
    {
        currentHealth = maxHealth;
        healthText.text = currentHealth.ToString();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        healthText.text = currentHealth.ToString();
        if (currentHealth <= 0)
            OnCastleDestroyed();
    }

    void OnCastleDestroyed()
    {
        Debug.Log("Castle Destroyed!");
    }
}
