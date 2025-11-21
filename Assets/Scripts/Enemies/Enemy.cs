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
    public float smokeDelay = 0.6f;
    public GameObject smokeEffectPrefab;
    public float destroyDelay = 1.2f;

    public bool IsDead => currentHealth <= 0f;
    private bool isDying = false;

    [Header("Config")]
    public EnemyType enemyType;

    private WaveSpawner spawner;   // cached reference
    private EnemyFollowPath followPath; // cached path script


    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        anim = GetComponentInChildren<Animator>();
        followPath = GetComponent<EnemyFollowPath>();
        spawner = FindFirstObjectByType<WaveSpawner>();

        if (enemyType != null)
        {
            maxHealth = enemyType.baseHealth;
            moveSpeed = enemyType.baseSpeed;
            rewardGold = enemyType.baseReward;
        }

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

        SpawnChildren();   // important: children spawn before destruction

        GrantReward();
        moveSpeed = 0f;

        if (anim != null)
            anim.SetTrigger("Bounce");

        StartCoroutine(DeathSequence());
    }


    // ===============================================================
    // CHILD SPAWNING — uses proper spawn override and path continuation
    // ===============================================================
    private void SpawnChildren()
    {
        if (enemyType == null || enemyType.spawnsOnDeath == null) return;
        if (spawner == null) return;

        TileNode parentNode = null;

        // Only works if this enemy actually has a followPath
        if (followPath != null && followPath.nextNode != null)
            parentNode = followPath.nextNode.GetComponent<TileNode>();

        foreach (var entry in enemyType.spawnsOnDeath)
        {
            if (entry.enemyToSpawn == null || entry.enemyToSpawn.prefab == null)
                continue;

            for (int i = 0; i < entry.count; i++)
            {
                // Spawn point around corpse
                Vector2 offset = Random.insideUnitCircle * entry.spreadRadius;
                Vector3 spawnPos = transform.position + (Vector3)offset;

                // Create enemy
                GameObject child = Instantiate(entry.enemyToSpawn.prefab, spawnPos, Quaternion.identity);

                // Assign type
                Enemy childEnemy = child.GetComponent<Enemy>();
                childEnemy.enemyType = entry.enemyToSpawn;

                // Path
                EnemyFollowPath childPath = child.GetComponent<EnemyFollowPath>();
                childPath.nextNode = this.followPath.nextNode;
                childPath.currentIndex = this.followPath.currentIndex;

                // Override spawn position to prevent teleporting
                childPath.SetSpawnPosition(spawnPos);

                if (parentNode != null)
                {
                    // Continue exactly from parent's next node
                    childPath.SetCurrentNode(parentNode);
                }
            }
        }
    }



    private IEnumerator DeathSequence()
    {
        yield return null;

        float animLength = 0.3f;

        if (anim != null)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            animLength = state.length;
        }

        yield return new WaitForSeconds(animLength + smokeDelay);

        if (smokeEffectPrefab != null)
        {
            GameObject smoke = Instantiate(
                smokeEffectPrefab,
                transform.position + Vector3.up * 0.1f,
                Quaternion.identity
            );
            Destroy(smoke, 2f);
        }

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
