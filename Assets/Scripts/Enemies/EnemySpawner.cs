using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public GameObject enemies;

    private List<GameObject> aliveEnemies = new List<GameObject>();

    // Called by WaveManager
    public IEnumerator SpawnWave(Wave wave)
    {
        aliveEnemies.Clear();

        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (enemyPrefab == null || spawnPoint == null)
            {
                Debug.LogError("EnemySpawner: Missing enemyPrefab or spawnPoint reference!");
                yield break;
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, enemies.transform);
            aliveEnemies.Add(enemy);

            // Apply scaling from the wave data
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.maxHealth *= wave.enemyHealthMultiplier;
                enemyScript.currentHealth = enemyScript.maxHealth;
                enemyScript.moveSpeed *= wave.enemySpeedMultiplier;
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    public bool HasAliveEnemies()
    {
        aliveEnemies.RemoveAll(e => e == null);
        return aliveEnemies.Count > 0;
    }
}