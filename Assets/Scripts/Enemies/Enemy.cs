using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float moveSpeed = 0.5f;
    public float rewardGold = 5f;
    public FloatingText floatingTextPrefab;
    
    [Header("Optional Path Progress")]
    public float pathProgress; // useful for "First" targeting logic later

    public bool IsDead => currentHealth <= 0f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        TowerPlacement playerEconomy = FindFirstObjectByType<TowerPlacement>();
        if (playerEconomy != null)
        {
            int waveIndex = FindFirstObjectByType<WaveManager>()?.currentWaveIndex ?? 0;
            float waveMultiplier = Mathf.RoundToInt(rewardGold * (1 + 0.15f * waveIndex));
            playerEconomy.AddGold(Mathf.RoundToInt(waveMultiplier));
            
    
        GameObject parent = GameObject.Find("Popup Texts");
        FloatingText popup = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity, parent != null ? parent.transform : null);
        popup.Initialize("+" + waveMultiplier.ToString());
    
        }
      
        Destroy(gameObject);
    }
}
