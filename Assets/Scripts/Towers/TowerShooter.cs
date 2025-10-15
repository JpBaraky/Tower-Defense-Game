using UnityEngine;

[RequireComponent(typeof(TowerTargeting))]
public class TowerShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float projectileSpeed = 15f;
    public float damage = 10f;

    private TowerTargeting targeting;
    private float fireCooldown;

    void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
    }

    void Update()
    {
        if (targeting == null || targeting.currentTarget == null)
            return;

        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / fireRate;
        }
    }

    private void Fire()
    {
        Enemy target = targeting.currentTarget;
        if (target == null)
            return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.Initialize(target.transform, damage, projectileSpeed);
        }
    }
}