using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // Enemy to spawn
    public Transform spawnPoint;        // Where enemies appear
    public int maxEnemies = 5;          // Max alive at once
    public float spawnInterval = 3f;    // Seconds between spawns

    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Remove destroyed enemies from the list
            aliveEnemies.RemoveAll(e => e == null);

            if (aliveEnemies.Count < maxEnemies)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                aliveEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}