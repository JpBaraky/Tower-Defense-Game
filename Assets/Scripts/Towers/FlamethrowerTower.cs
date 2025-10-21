using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class FlamethrowerTower : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(5f, 180f)] public float coneAngle = 45f;
    [Min(0f)] public float coneLength = 8f;

    [Header("Damage Settings")]
    public float towerDamage = 20f;
    public bool applyBurn = true;
    public float burnDamage = 5f;
    public float burnDuration = 2f;
    public float damageInterval = 0.1f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    [Header("Visuals")]
    public ParticleSystem flameEffect;
    public GameObject burnEffectPrefab;

    private TowerTargeting targeting;
    private float damageTimer;

    private class BurnInfo
    {
        public float timeLeft;
        public GameObject fireVFX;
    }

    private readonly Dictionary<Enemy, BurnInfo> burningEnemies = new();

    private void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
    }

    private void Update()
    {
        // Skip gameplay logic while in Editor and not playing
        if (!Application.isPlaying)
        {
            UpdateConeVisualsInEditor();
            return;
        }

        Enemy target = targeting.currentTarget;

        if (target == null)
        {
            StopFlame();
            return;
        }

        if (flameEffect != null && !flameEffect.isPlaying)
            flameEffect.Play();

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyConeDamage();
            damageTimer = 0f;
        }

        UpdateBurningEnemies();
    }

    private void ApplyConeDamage()
    {
        Vector3 forward = transform.forward;
        Vector3 origin = transform.position;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.isActiveAndEnabled)
                continue;

            Vector3 dir = e.transform.position - origin;
            float dist = dir.magnitude;
            if (dist > coneLength) continue;

            float angle = Vector3.Angle(forward, dir);
            if (angle > coneAngle / 2f) continue;

            e.TakeDamage(towerDamage * damageInterval);

            if (applyBurn)
            {
                if (burningEnemies.TryGetValue(e, out var info))
                {
                    info.timeLeft = burnDuration;
                }
                else
                {
                    GameObject fx = null;
                    if (burnEffectPrefab)
                    {
                        fx = Instantiate(burnEffectPrefab, e.transform);
                       // fx.transform.localPosition = Vector3.up * 1f;
                    }

                    burningEnemies[e] = new BurnInfo
                    {
                        timeLeft = burnDuration,
                        fireVFX = fx
                    };
                }
            }
        }
    }

    private void UpdateBurningEnemies()
    {
        if (burningEnemies.Count == 0) return;

        List<Enemy> toRemove = new();

        foreach (var kvp in burningEnemies)
        {
            Enemy e = kvp.Key;
            BurnInfo info = kvp.Value;

            if (e == null || !e.isActiveAndEnabled)
            {
                if (info.fireVFX) Destroy(info.fireVFX);
                toRemove.Add(e);
                continue;
            }

            e.TakeDamage(burnDamage * Time.deltaTime);
            info.timeLeft -= Time.deltaTime;

            if (info.timeLeft <= 0)
            {
                if (info.fireVFX) Destroy(info.fireVFX);
                toRemove.Add(e);
            }
        }

        foreach (var e in toRemove)
            burningEnemies.Remove(e);
    }

    private void StopFlame()
    {
        if (flameEffect != null && flameEffect.isPlaying)
            flameEffect.Stop();
    }

    // --- Live editor visualization ---
    private void UpdateConeVisualsInEditor()
    {
        if (flameEffect != null)
        {
            var shape = flameEffect.shape;
            shape.angle = coneAngle - 10;
            shape.radius = coneLength * 0.1f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Application.isPlaying ? Color.red : Color.yellow;
        Vector3 forward = transform.forward;

        Quaternion leftRayRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRayRot = Quaternion.Euler(0, coneAngle / 2f, 0);

        Vector3 leftDir = leftRayRot * forward;
        Vector3 rightDir = rightRayRot * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * coneLength);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * coneLength);

        int segments = 20;
        Vector3 prevPoint = transform.position + leftDir * coneLength;
        for (int i = 1; i <= segments; i++)
        {
            float lerp = i / (float)segments;
            Quaternion rot = Quaternion.Euler(0, -coneAngle / 2f + lerp * coneAngle, 0);
            Vector3 dir = rot * forward;
            Vector3 nextPoint = transform.position + dir * coneLength;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
