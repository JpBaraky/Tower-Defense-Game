using System.Collections.Generic;
using System.Linq;
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

    void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = range;
    }

    void Update()
    {
        CleanEnemyList();

        if (currentTarget == null)
            currentTarget = SelectTarget();

        if (currentTarget != null)
        {
            if (rotateTowardsTarget)
                RotateTowards(currentTarget.transform);

            // Optional: check if target left range or died
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (dist > range || !currentTarget.isActiveAndEnabled)
                currentTarget = null;
        }
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

        switch (targetMode)
        {
            case TargetMode.Closest:
                return enemiesInRange.OrderBy(e => Vector3.Distance(transform.position, e.transform.position)).FirstOrDefault();

            case TargetMode.Weakest:
                return enemiesInRange.OrderBy(e => e.currentHealth).FirstOrDefault();

            case TargetMode.Strongest:
                return enemiesInRange.OrderByDescending(e => e.currentHealth).FirstOrDefault();

            case TargetMode.Random:
                return enemiesInRange[Random.Range(0, enemiesInRange.Count)];

            case TargetMode.First:
            default:
                // "First" = the one that entered the range first
                return enemiesInRange[0];
        }
    }

    private void CleanEnemyList()
    {
        enemiesInRange.RemoveAll(e => e == null || !e.isActiveAndEnabled);
    }

    void OnTriggerEnter(Collider other)
    {
        
        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null && !enemiesInRange.Contains(e))
            enemiesInRange.Add(e);
             Debug.Log($"Enemy entered: {e.name}");
    }

    void OnTriggerExit(Collider other)
    {
        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null)
        {
            enemiesInRange.Remove(e);
            if (e == currentTarget)
                currentTarget = null;
                Debug.Log($"Enemy exited: {e.name}");
        }
    }
    

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}
