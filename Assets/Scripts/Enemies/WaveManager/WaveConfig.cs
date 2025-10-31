using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "TD/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Scaling Modifiers")]
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float rewardMultiplier = 1f;

    [Header("Enemies in this Wave")]
    public List<WaveEnemy> enemies = new List<WaveEnemy>();
}

[System.Serializable]
public class WaveEnemy
{
    public EnemyType enemyType;
    public int count = 10;
}