using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class FrostTower : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(90, 360)] public float coneAngle = 360f;
    [Min(0f)] public float coneLength = 1f;

    [Header("Damage Settings")]
    public float towerDamage = 10f;
    public bool applySlow = true;
    [Range(0, 90)] public float slowPercent = 30f;
    [Min(0f)] public float slowDuration = 2f;
    [Min(0.01f)] public float damageInterval = 0.1f;

    [Header("Visuals")]
    public ParticleSystem frostEffect;
    public Color slowColor = new(0.3f, 0.6f, 1f);

    private TowerTargeting targeting;
    private float damageTimer;

    private class SlowInfo
    {
        public int towerCount; // how many towers are currently applying the slow
        public float timeLeft;
        public float originalSpeed;
        public Renderer[] renderers;
        public Color[] originalColors;
    }

    // Shared across all towers to handle multiple sources
    private static readonly Dictionary<Enemy, SlowInfo> slowedEnemies = new();

    private void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
    }

    private void Update()
    {
        if (!Application.isPlaying || targeting.isPreview)
        {
            UpdateConeVisualsInEditor();
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
        RotateHead();
    }

    private void ApplyConeEffect()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.isActiveAndEnabled) continue;

            Vector3 dir = e.transform.position - origin;
            if (dir.magnitude > coneLength) continue;
            if (Vector3.Angle(forward, dir) > coneAngle / 2f) continue;

            // Apply damage
            e.TakeDamage(towerDamage * damageInterval * (1 + targeting.heightStep / 10f));

            if (!applySlow) continue;

            float newDuration = slowDuration * (1 + targeting.heightStep / 10f);

            if (slowedEnemies.TryGetValue(e, out var info))
            {
                // Already slowed: increment tower count and refresh duration if higher
                info.towerCount++;
                if (newDuration > info.timeLeft)
                    info.timeLeft = newDuration;
            }
            else
            {
                // First tower to slow this enemy
                Renderer[] renderers = e.GetComponentsInChildren<Renderer>();
                Color[] originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                    originalColors[i] = renderers[i].material.color;

                float originalSpeed = e.moveSpeed;
                e.moveSpeed *= Mathf.Clamp01(1f - slowPercent * (1 + targeting.heightStep / 10f) / 100f);

                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].material.color = slowColor;

                slowedEnemies[e] = new SlowInfo
                {
                    towerCount = 1,
                    timeLeft = newDuration,
                    originalSpeed = originalSpeed,
                    renderers = renderers,
                    originalColors = originalColors
                };
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
                RestoreEnemy(e, info);
                toRemove.Add(e);
                continue;
            }

            // Count down duration only if no tower applied this frame
            if (info.towerCount == 0)
                info.timeLeft -= Time.deltaTime;

            if (info.timeLeft <= 0f)
            {
                RestoreEnemy(e, info);
                toRemove.Add(e);
            }
            else
            {
                // Reset towerCount for next frame, each tower will re-apply
                info.towerCount = 0;
            }
        }

        foreach (var e in toRemove)
            slowedEnemies.Remove(e);
    }

    private void RestoreEnemy(Enemy e, SlowInfo info)
    {
        if (e == null) return;

        e.moveSpeed = info.originalSpeed;

        if (info.renderers != null)
        {
            for (int i = 0; i < info.renderers.Length; i++)
                info.renderers[i].material.color = info.originalColors[i];
        }
    }

    private void UpdateConeVisualsInEditor()
    {
        if (frostEffect == null) return;

        var shape = frostEffect.shape;
        shape.angle = coneAngle;

        var main = frostEffect.main;
        float startLifetime = Mathf.Sqrt(coneLength);
        float startSpeed = (coneLength * 10f) / startLifetime;
        main.startSpeed = startSpeed;
        main.startLifetime = startLifetime;

        var emission = frostEffect.emission;
        emission.rateOverTime = coneLength * 25f;
    }

    public void SetConeLength(float length)
    {
        coneLength = length;
        UpdateConeVisualsInEditor();
    }

#if UNITY_EDITOR
    public Transform towerHead;
    public Transform firePoint;

    private void OnDrawGizmos()
    {
        SetConeLength(targeting != null ? targeting.range : coneLength);
        Gizmos.color = Application.isPlaying ? Color.cyan : Color.blue;

        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 forward = towerHead != null ? towerHead.forward : transform.forward;

        Quaternion leftRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, coneAngle / 2f, 0);
        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(startPos, startPos + leftDir * coneLength);
        Gizmos.DrawLine(startPos, startPos + rightDir * coneLength);

        int segments = 40;
        Vector3 prevPoint = startPos + leftDir * coneLength;
        for (int i = 1; i <= segments; i++)
        {
            float lerp = i / (float)segments;
            Quaternion rot = Quaternion.Euler(0, -coneAngle / 2f + lerp * coneAngle, 0);
            Vector3 dir = rot * forward;
            Vector3 nextPoint = startPos + dir * coneLength;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    private void RotateHead()
    {
        if (towerHead == null) return;
        towerHead.Rotate(0, 180f * Time.deltaTime, 0f, Space.Self);
    }
#endif
}
