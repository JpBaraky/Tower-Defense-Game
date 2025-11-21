using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyType", menuName = "TD/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Info")]
    public string enemyName = "New Enemy";
    public GameObject prefab;

    [Header("Base Stats")]
    public float baseHealth = 100f;
    public float baseSpeed = 3f;
    public float baseDamage = 10f;
    public int baseReward = 5;

    [Header("Special Flags")]
    public bool isFlying = false;
    public bool isBoss = false;

    [Header("Spawn On Death")]
    public SpawnEntry[] spawnsOnDeath;
}

[System.Serializable]
public struct SpawnEntry
{
    public EnemyType enemyToSpawn;   // Which enemy type to spawn
    public int count;                // How many
    public float spreadRadius;       // Offset radius around corpse
}