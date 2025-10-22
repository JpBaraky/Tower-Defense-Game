using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class FrostTower : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(90, 360)] public float coneAngle = 360f;
    [Min(0f)] public float coneLength = 8f;
    private WaveManager waveManager;

    [Header("Damage Settings")]
    public float towerDamage = 10f;
    public bool applySlow = true;
    [Tooltip("Percent speed reduction (e.g. 10 = 10% slower)")]
    [Range(0, 90)] public float slowPercent = 30f;
    [Min(0f)] public float slowDuration = 2f;
    [Min(0.01f)] public float damageInterval = 0.1f;

    [Header("Visuals")]
    public ParticleSystem frostEffect;
    public Color slowColor = new Color(0.1f, 0.3f, 1f); // dark blue tint

    private TowerTargeting targeting;
    private float damageTimer;

    private class SlowInfo
    {
        public float timeLeft;
        public float originalSpeed;
        public Renderer renderer;
        public Color originalColor;
    }

    private readonly Dictionary<Enemy, SlowInfo> slowedEnemies = new();

    private void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
        waveManager = FindFirstObjectByType<WaveManager>();
    }

    private void Update()
    {
        // Skip logic in editor mode
        if (!Application.isPlaying)
        {
            UpdateConeVisualsInEditor();
            return;
        }

        Enemy target = targeting.currentTarget;

        if (waveManager.waveInProgress == false )
        {
            StopFrost();
            return;
        }

        if (frostEffect != null && !frostEffect.isPlaying)
            frostEffect.Play();

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyConeEffect();
            damageTimer = 0f;
        }

        UpdateSlowedEnemies();
    }

    private void ApplyConeEffect()
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

            // Base tower damage
            e.TakeDamage(towerDamage * damageInterval);

            if (applySlow)
            {
                if (slowedEnemies.TryGetValue(e, out var info))
                {
                    info.timeLeft = slowDuration;
                }
                else
                {
                    Renderer r = e.GetComponentInChildren<Renderer>();
                    float originalSpeed = e.moveSpeed; // requires Enemy to have moveSpeed public
                    Color originalColor = r != null ? r.material.color : Color.white;

                    // Apply visual + speed slow
                    e.moveSpeed *= (1f - slowPercent / 100f);
                    if (r != null)
                        r.material.color = slowColor;

                    slowedEnemies[e] = new SlowInfo
                    {
                        timeLeft = slowDuration,
                        originalSpeed = originalSpeed,
                        renderer = r,
                        originalColor = originalColor
                    };
                }
            }
        }
    }

    private void UpdateSlowedEnemies()
    {
        if (slowedEnemies.Count == 0) return;

        List<Enemy> toRemove = new();

        foreach (var kvp in slowedEnemies)
        {
            Enemy e = kvp.Key;
            SlowInfo info = kvp.Value;

            if (e == null || !e.isActiveAndEnabled)
            {
                RestoreEnemyState(e, info);
                toRemove.Add(e);
                continue;
            }

            info.timeLeft -= Time.deltaTime;
            if (info.timeLeft <= 0)
            {
                RestoreEnemyState(e, info);
                toRemove.Add(e);
            }
        }

        foreach (var e in toRemove)
            slowedEnemies.Remove(e);
    }

    private void RestoreEnemyState(Enemy e, SlowInfo info)
    {
        if (e == null) return;
        e.moveSpeed = info.originalSpeed;
        if (info.renderer != null)
            info.renderer.material.color = info.originalColor;
    }

    private void StopFrost()
    {
        if (frostEffect != null && frostEffect.isPlaying)
            frostEffect.Stop();
    }

    private void UpdateConeVisualsInEditor()
    {
        if (frostEffect != null)
        {
            var shape = frostEffect.shape;
            shape.angle = coneAngle - 10;
            shape.radius = coneLength * 0.1f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Application.isPlaying ? Color.cyan : Color.blue;
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
