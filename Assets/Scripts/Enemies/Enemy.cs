using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float moveSpeed = 0.5f;
    public float rewardGold = 5f;
    public FloatingText floatingTextPrefab;

    private EnemyStats enemyStats;
    private Animator anim;

    [Header("Death Settings")]
    public float smokeDelay = 0.6f;        // Time after animation ends
    public GameObject smokeEffectPrefab;   // Poof effect
    public float destroyDelay = 1.2f;      // After smoke spawn

    public bool IsDead => currentHealth <= 0f;
    private bool isDying = false;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        anim = GetComponentInChildren<Animator>();

        maxHealth = enemyStats.health;
        moveSpeed = enemyStats.speed;
        rewardGold = enemyStats.reward;

        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDying) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;

        GrantReward();

        // Stop movement immediately
        moveSpeed = 0f;

        // Trigger death animation
        if (anim != null)
            anim.SetTrigger("Bounce");

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Wait for animation state to appear
        yield return null;

        // Wait until death animation fully ends
        float animLength = 0.3f;

        if (anim != null)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            animLength = state.length;
        }

        yield return new WaitForSeconds(animLength + smokeDelay);

        // Spawn smoke effect
        if (smokeEffectPrefab != null)
        {
          GameObject smoke =  Instantiate(
                smokeEffectPrefab,
                transform.position + Vector3.up * 0.1f,
                Quaternion.identity
            );
            Destroy(smoke, 2f);
        }

        // Destroy after optional delay
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }

    private void GrantReward()
    {
        TowerPlacement econ = FindFirstObjectByType<TowerPlacement>();
        if (econ == null) return;

        int gold = Mathf.RoundToInt(rewardGold);
        econ.AddGold(gold);

        GameObject parent = GameObject.Find("Popup Texts");
        FloatingText popup = Instantiate(
            floatingTextPrefab,
            transform.position + Vector3.up * 0.1f,
            Quaternion.identity,
            parent != null ? parent.transform : null
        );
        popup.Initialize("+" + gold);
    }
}
