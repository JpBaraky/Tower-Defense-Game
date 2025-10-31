using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float moveSpeed = 0.5f;
    public float rewardGold = 5f;
    public FloatingText floatingTextPrefab;
    private EnemyStats enemyStats;
    
    [Header("Optional Path Progress")]
    public float pathProgress; // useful for "First" targeting logic later

    public bool IsDead => currentHealth <= 0f;

    void Start()
    {
       
       
        enemyStats = GetComponent<EnemyStats>();
        maxHealth = enemyStats.health;
        moveSpeed = enemyStats.speed;
        rewardGold = enemyStats.reward;
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
           
            float waveMultiplier = Mathf.RoundToInt(rewardGold);
            playerEconomy.AddGold(Mathf.RoundToInt(waveMultiplier));
            
    
        GameObject parent = GameObject.Find("Popup Texts");
        FloatingText popup = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity, parent != null ? parent.transform : null);
        popup.Initialize("+" + waveMultiplier.ToString());
    
        }
      
        Destroy(gameObject);
    }
}
