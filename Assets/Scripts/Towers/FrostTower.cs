using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class FrostTower : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(90, 360)] public float coneAngle = 360f;
    [Min(0f)] public float coneLength = 1f;
    private WaveSpawner waveSpawner;

    [Header("Damage Settings")]
    public float towerDamage = 10f;
    public bool applySlow = true;
    [Range(0, 90)] public float slowPercent = 30f;
    [Min(0f)] public float slowDuration = 2f;
    [Min(0.01f)] public float damageInterval = 0.1f;

    [Header("Visuals")]
    public ParticleSystem frostEffect;
    public Color slowColor = new Color(0.3f, 0.6f, 1f);

    private TowerTargeting targeting;
    private float damageTimer;

    private class SlowInfo
    {
        public float timeLeft;
        public float originalSpeed;
        public Renderer renderer;
        public Color originalColor;
        public bool refreshedThisFrame;
    }

    private readonly Dictionary<Enemy, SlowInfo> slowedEnemies = new();

    private void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
    }

    private void Update()
    {
        if (!Application.isPlaying || targeting.isPreview)
        {
            UpdateConeVisualsInEditor();
            return;
        }

        if (waveSpawner == null || !waveSpawner.waveInProgress)
        {
            StopFrost();
            ClearAllSlows();
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

        // Mark all slowed enemies as not refreshed this frame
        foreach (var info in slowedEnemies.Values)
            info.refreshedThisFrame = false;

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.isActiveAndEnabled)
                continue;

            Vector3 dir = e.transform.position - origin;
            float dist = dir.magnitude;
            if (dist > coneLength) continue;

            float angle = Vector3.Angle(forward, dir);
            if (angle > coneAngle / 2f) continue;

            // Damage tick
            e.TakeDamage(towerDamage * damageInterval * (1 + targeting.heightStep / 10f));

            // Slow logic
            if (!applySlow) continue;

            if (slowedEnemies.TryGetValue(e, out var info))
            {
                info.timeLeft = slowDuration * (1 + targeting.heightStep / 10f);
                info.refreshedThisFrame = true;
            }
            else
            {
                Renderer r = e.GetComponentInChildren<Renderer>();
                float originalSpeed = e.moveSpeed;
                Color originalColor = r != null ? r.material.color : Color.white;

                e.moveSpeed *= (1f - slowPercent * (1 + targeting.heightStep / 10f) / 100f);
                if (r != null)
                    r.material.color = slowColor;

                slowedEnemies[e] = new SlowInfo
                {
                    timeLeft = slowDuration * (1 + targeting.heightStep / 10f),
                    originalSpeed = originalSpeed,
                    renderer = r,
                    originalColor = originalColor,
                    refreshedThisFrame = true
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
                RestoreEnemyState(e, info);
                toRemove.Add(e);
                continue;
            }

            // Decrease timer only if not refreshed this frame
            if (!info.refreshedThisFrame)
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

    private void ClearAllSlows()
    {
        foreach (var kvp in slowedEnemies)
            RestoreEnemyState(kvp.Key, kvp.Value);
        slowedEnemies.Clear();
    }

    private void StopFrost()
    {
        if (frostEffect != null && frostEffect.isPlaying)
            frostEffect.Stop();
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

        Quaternion leftRayRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRayRot = Quaternion.Euler(0, coneAngle / 2f, 0);
        Vector3 leftDir = leftRayRot * forward;
        Vector3 rightDir = rightRayRot * forward;

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
    void RotateHead(){
     
 
       
        float rotY = 180 * Time.deltaTime;

        towerHead.Rotate(0, rotY, 0f, Space.Self);
    } 

    
#endif
}
