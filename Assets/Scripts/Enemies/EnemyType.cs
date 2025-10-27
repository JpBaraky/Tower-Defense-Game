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

    [Header("Special")]
    public bool isFlying = false;
    public bool isBoss = false;
}