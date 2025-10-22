using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public List<Wave> waves = new List<Wave>();
    public float timeBetweenWaves = 5f;

    public int currentWaveIndex = 0;
    public bool waveInProgress = false;

    void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(5f); // Small delay before first wave

        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];
            waveInProgress = true;

            Debug.Log($"Starting Wave {currentWaveIndex + 1}");
            yield return StartCoroutine(spawner.SpawnWave(currentWave));

            // Wait until all enemies are dead
            while (spawner.HasAliveEnemies())
                yield return null;

            waveInProgress = false;
            Debug.Log($"Wave {currentWaveIndex + 1} completed!");

            currentWaveIndex++;
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        Debug.Log("All waves completed!");
    }
}
