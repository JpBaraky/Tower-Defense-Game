using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<WaveConfig> waves;
    public Transform spawnPoint;
    public float spawnDelay = 0.5f;

    [Header("References")]
    public HandManager handManager;
    public ResourceManager resourceManager;
    public GameObject waveButtonUI;
    public TMP_Text waveButtonLabel;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private int activeEnemies = 0;

    private WaveConfig currentWave;   // <<< NECESSARY FOR CHILDREN

    public bool WaveInProgress => isSpawning || activeEnemies > 0;

    void Start()
    {
        // initial state
        resourceManager.SetMana(3);

        for (int i = 0; i < 5; i++)
            handManager.DrawCard();

        UpdateWaveButtonLabel();
    }

    void Update()
    {
        waveButtonUI.SetActive(!WaveInProgress);
    }

    public void StartWaveButton()
    {
        if (WaveInProgress) return;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (!isSpawning && currentWaveIndex < waves.Count)
            StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;

        currentWave = waves[currentWaveIndex];   // <<< IMPORTANT

        foreach (var waveEnemy in currentWave.enemies)
        {
            for (int i = 0; i < waveEnemy.count; i++)
            {
                SpawnEnemy(waveEnemy.enemyType, currentWave);
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        isSpawning = false;
        currentWaveIndex++;

        UpdateWaveButtonLabel();
    }

    private void SpawnEnemy(EnemyType type, WaveConfig wave)
    {
        if (type == null || type.prefab == null) return;

        GameObject enemy = Instantiate(type.prefab, spawnPoint.position, Quaternion.identity);

        Enemy enemyComp = enemy.GetComponent<Enemy>();
        enemyComp.enemyType = type;

        EnemyStats stats = enemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.health = type.baseHealth * wave.healthMultiplier;
            stats.speed = type.baseSpeed * wave.speedMultiplier;
            stats.reward = Mathf.RoundToInt(type.baseReward * wave.rewardMultiplier);
        }

        activeEnemies++;

        EnemyDeathHandler dh = enemy.AddComponent<EnemyDeathHandler>();
        dh.spawner = this;
    }

    public void OnEnemyDeath()
    {
        activeEnemies--;

        if (activeEnemies <= 0 && !isSpawning)
            EndWave();
    }

    private void EndWave()
    {
        DiscardEntireHand();
        DrawInitialHand();
        resourceManager.SetMana(3);

        UpdateWaveButtonLabel();
    }

    private void DrawInitialHand()
    {
        for (int i = 0; i < 5; i++)
            handManager.DrawCard();
    }

    private void DiscardEntireHand()
    {
        var temp = new List<Card>(handManager.CurrentHand);
        foreach (var c in temp)
            handManager.DiscardCard(c);
    }

    private void UpdateWaveButtonLabel()
    {
        if (waveButtonLabel == null) return;

        if (currentWaveIndex >= waves.Count)
            waveButtonLabel.text = "No More Waves";
        else
            waveButtonLabel.text = $"Start Wave {currentWaveIndex + 1}";
    }


    // ============================================================
    // CHILD ENEMY SPAWNING
    // ============================================================
    public void SpawnChildEnemy(EnemyType type, Vector3 position)
    {
        if (type == null || type.prefab == null) return;

        GameObject enemyObj = Instantiate(type.prefab, position, Quaternion.identity);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemy.enemyType = type;

        // Select correct wave
        WaveConfig waveToUse;

        if (currentWaveIndex >= waves.Count)
            waveToUse = currentWave;            // still use last wave multipliers
        else if (currentWave != null)
            waveToUse = currentWave;
        else
            waveToUse = waves[currentWaveIndex];

        // Apply stats
        EnemyStats stats = enemyObj.GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.health = type.baseHealth * waveToUse.healthMultiplier;
            stats.speed = type.baseSpeed * waveToUse.speedMultiplier;
            stats.reward = Mathf.RoundToInt(type.baseReward * waveToUse.rewardMultiplier);
        }

        activeEnemies++;

        EnemyDeathHandler dh = enemyObj.AddComponent<EnemyDeathHandler>();
        dh.spawner = this;
    }
}
