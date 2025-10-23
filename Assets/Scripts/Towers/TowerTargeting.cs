using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TowerTargeting : MonoBehaviour
{
    public enum TargetMode { First, Closest, Weakest, Strongest, Random }

    [Header("Settings")]
    public TargetMode targetMode = TargetMode.First;
    public float range = 10f;
    public bool rotateTowardsTarget = true;
    public float rotationSpeed = 5f;

    [Header("References")]
    public Transform towerHead; // Optional, where rotation happens
    private SphereCollider detectionCollider;

    private List<Enemy> enemiesInRange = new List<Enemy>();
    public Enemy currentTarget;

    // Flag to indicate the enemy list has changed
    private bool enemiesChanged = false;
    public bool isPreview;

    void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = range;
    }

    void Update()
    {
        if (isPreview)
            return;
        CleanEnemyList();

        // Only recalculate target if the list changed or current target is invalid
        if (currentTarget == null || !enemiesInRange.Contains(currentTarget) || enemiesChanged)
        {
            currentTarget = SelectTarget();
            enemiesChanged = false;
        }

        if (currentTarget != null && rotateTowardsTarget)
            RotateTowards(currentTarget.transform);
    }

    private void RotateTowards(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

        if (towerHead != null)
            towerHead.rotation = Quaternion.Lerp(towerHead.rotation, lookRot, Time.deltaTime * rotationSpeed);
        else
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
    }

    private Enemy SelectTarget()
    {
        if (enemiesInRange.Count == 0)
            return null;

        Enemy selected = null;

        switch (targetMode)
        {
            case TargetMode.Closest:
                float minDist = float.MaxValue;
                foreach (var e in enemiesInRange)
                {
                    if (e == null || !e.isActiveAndEnabled) continue;
                    float dist = Vector3.Distance(transform.position, e.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        selected = e;
                    }
                }
                break;

            case TargetMode.Weakest:
                float minHealth = float.MaxValue;
                foreach (var e in enemiesInRange)
                {
                    if (e == null || !e.isActiveAndEnabled) continue;
                    if (e.currentHealth < minHealth)
                    {
                        minHealth = e.currentHealth;
                        selected = e;
                    }
                }
                break;

            case TargetMode.Strongest:
                float maxHealth = float.MinValue;
                foreach (var e in enemiesInRange)
                {
                    if (e == null || !e.isActiveAndEnabled) continue;
                    if (e.currentHealth > maxHealth)
                    {
                        maxHealth = e.currentHealth;
                        selected = e;
                    }
                }
                break;

            case TargetMode.Random:
                int attempts = 0;
                while (attempts < enemiesInRange.Count)
                {
                    var candidate = enemiesInRange[Random.Range(0, enemiesInRange.Count)];
                    if (candidate != null && candidate.isActiveAndEnabled)
                    {
                        selected = candidate;
                        break;
                    }
                    attempts++;
                }
                break;

            case TargetMode.First:
            default:
                foreach (var e in enemiesInRange)
                {
                    if (e != null && e.isActiveAndEnabled)
                    {
                        selected = e;
                        break;
                    }
                }
                break;
        }

        return selected;
    }

    private void CleanEnemyList()
    {
        bool removed = false;

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null || !enemiesInRange[i].isActiveAndEnabled)
            {
                enemiesInRange.RemoveAt(i);
                removed = true;
            }
        }

        if (removed || (currentTarget != null && !enemiesInRange.Contains(currentTarget)))
            enemiesChanged = true;

        if (currentTarget != null && !enemiesInRange.Contains(currentTarget))
            currentTarget = null;
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null && !enemiesInRange.Contains(e))
        {
            enemiesInRange.Add(e);
            enemiesChanged = true;
            Debug.Log($"Enemy entered: {e.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null)
        {
            enemiesInRange.Remove(e);
            enemiesChanged = true;
            if (e == currentTarget)
                currentTarget = null;
            Debug.Log($"Enemy exited: {e.name}");
        }
    }
}
