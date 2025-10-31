using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("List of WaveConfig ScriptableObjects that define each wave.")]
    public List<WaveConfig> waves;

    [Header("Spawn Settings")]
    [Tooltip("Where enemies will spawn from.")]
    public Transform spawnPoint;

    [Tooltip("Delay between each enemy spawn.")]
    public float spawnDelay = 0.5f;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    public bool waveInProgress => isSpawning || activeEnemies > 0;

    // Used to keep track of how many enemies are alive
    private int activeEnemies = 0;
    private void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (!isSpawning)
            StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("✅ All waves complete!");
            yield break;
        }

        isSpawning = true;
        WaveConfig wave = waves[currentWaveIndex];
        Debug.Log($"⚔️ Spawning Wave {currentWaveIndex + 1}");

        foreach (var waveEnemy in wave.enemies)
        {
            for (int i = 0; i < waveEnemy.count; i++)
            {
                SpawnEnemy(waveEnemy.enemyType, wave);
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        isSpawning = false;
        currentWaveIndex++;
    }

    private void SpawnEnemy(EnemyType type, WaveConfig wave)
    {
        if (type == null || type.prefab == null)
        {
            Debug.LogWarning($"⚠️ Missing EnemyType or prefab in Wave {currentWaveIndex + 1}");
            return;
        }

        GameObject enemy = Instantiate(type.prefab, spawnPoint.position, Quaternion.identity);

        // Apply scaled stats
        EnemyStats stats = enemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.health = type.baseHealth * wave.healthMultiplier;
            stats.speed = type.baseSpeed * wave.speedMultiplier;
            stats.reward = Mathf.RoundToInt(type.baseReward * wave.rewardMultiplier);
        }

        // Track enemies if needed
        activeEnemies++;
        EnemyDeathHandler deathHandler = enemy.AddComponent<EnemyDeathHandler>();
        deathHandler.spawner = this;
    }

    public void OnEnemyDeath()
    {
        activeEnemies--;
        if (activeEnemies <= 0 && !isSpawning)
        {
            Debug.Log("Wave cleared!");
            StartNextWave(); // Automatically start the next wave
        }
    }
}
