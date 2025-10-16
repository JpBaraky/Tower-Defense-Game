using UnityEngine;

[RequireComponent(typeof(TowerTargeting))]
[RequireComponent(typeof(AudioSource))]
public class TowerShooter : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float projectileSpeed = 15f;
    public float damage = 10f;

    [Header("Audio")]
    public AudioClip shootClip;
    public float shootVolume = 1f;

    private TowerTargeting targeting;
    private AudioSource audioSource;
    private float fireCooldown;

    void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
        audioSource = GetComponent<AudioSource>();

        // optional setup if not configured in Inspector
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
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

        // spawn projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.Initialize(target.transform, damage, projectileSpeed);
        }

        // play sound
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip, shootVolume);
        }
    }
}