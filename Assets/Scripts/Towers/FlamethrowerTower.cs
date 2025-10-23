using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class FlamethrowerTower : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(20, 90)] public float coneAngle = 45f;
    [Min(0f)] public float coneLength = 1f;
    public Transform firePoint;

    [Header("Damage Settings")]
    public float towerDamage = 20f;
    public bool applyFire = true;
    public float FireDamage = 5f;
    public float FireDuration = 2f;
    public float damageInterval = 0.1f;
    public float FireVFXFadeTime = 0.5f; // Duration for VFX to fade

    [Header("Visuals")]
    public ParticleSystem FireEffect;
    public GameObject FireEffectPrefab;

    private TowerTargeting targeting;
    private float damageTimer;
    private Coroutine stopFireCoroutine;


    private class FireInfo
    {
        public float timeLeft;
        public GameObject FireVFX;
        public Coroutine fadeCoroutine;
    }

    private readonly Dictionary<Enemy, FireInfo> FireedEnemies = new();

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
        // Start delayed stop if not already running
        if (stopFireCoroutine == null)
            stopFireCoroutine = StartCoroutine(DelayedStopFire(0.1f));
    }
    else
    {
        // Cancel delayed stop if target came back
        if (stopFireCoroutine != null)
        {
            StopCoroutine(stopFireCoroutine);
            stopFireCoroutine = null;
        }

        // Play Fire effect if not already playing
        if (FireEffect != null && !FireEffect.isPlaying)
            FireEffect.Play();

        // Apply damage over time
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyConeDamage();
            damageTimer = 0f;
        }
    }
        // Always update Fireed enemies to remove expired VFX
        UpdateFireedEnemies();
    }
    private IEnumerator DelayedStopFire(float delay)
{
    yield return new WaitForSeconds(delay);
    StopFire();
    stopFireCoroutine = null;
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

            if (applyFire)
            {
                if (FireedEnemies.TryGetValue(e, out var info))
                {
                    info.timeLeft = FireDuration; // Refresh Fire duration
                }
                else
                {
                    GameObject fx = null;
                    if (FireEffectPrefab)
                    {
                        fx = Instantiate(FireEffectPrefab, e.transform);
                        fx.transform.localPosition = Vector3.zero;
                    }

                    FireedEnemies[e] = new FireInfo
                    {
                        timeLeft = FireDuration,
                        FireVFX = fx,
                        fadeCoroutine = null
                    };
                }
            }
        }
    }

    private void UpdateFireedEnemies()
    {
        if (FireedEnemies.Count == 0) return;

        List<Enemy> toRemove = new();

        foreach (var kvp in FireedEnemies)
        {
            Enemy e = kvp.Key;
            FireInfo info = kvp.Value;

            bool removeEnemy = false;

            if (e == null || !e.isActiveAndEnabled)
            {
                removeEnemy = true;
            }
            else
            {
                e.TakeDamage(FireDamage * Time.deltaTime);
                info.timeLeft -= Time.deltaTime;

                if (info.timeLeft <= 0)
                    removeEnemy = true;
            }

            if (removeEnemy)
            {
                if (info.FireVFX != null)
                {
                    if (info.fadeCoroutine != null)
                        StopCoroutine(info.fadeCoroutine);

                    info.fadeCoroutine = StartCoroutine(FadeAndDestroy(info.FireVFX, FireVFXFadeTime));
                    info.FireVFX = null;
                }

                toRemove.Add(e);
            }
        }

        foreach (var e in toRemove)
            FireedEnemies.Remove(e);

        if (FireedEnemies.Count == 0 && FireEffect != null && FireEffect.isPlaying)
            FireEffect.Stop();
    }

    private IEnumerator FadeAndDestroy(GameObject vfx, float duration)
    {
        if (vfx == null) yield break;

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        float elapsed = 0f;

        if (ps != null)
        {
            var main = ps.main;
            float startRate = main.startLifetime.constant;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        Destroy(vfx);
    }

    private void StopFire()
    {
        if (FireEffect != null && FireEffect.isPlaying)
            FireEffect.Stop();
    }

    private void UpdateConeVisualsInEditor()
    {
        if (FireEffect != null)
        {
            var shape = FireEffect.shape;
            shape.angle = coneAngle / 2;
            
             var main = FireEffect.main; 

float startLifetime = Mathf.Sqrt(coneLength);              // grows slower than linear
float startSpeed = (coneLength * 10f) / startLifetime;   


// Apply to particle system
main.startSpeed = startSpeed;
main.startLifetime = startLifetime;

//adjust emission to maintain density
var emission = FireEffect.emission;
emission.rateOverTime = coneLength * 25f;
        }
    }
    public void SetConeLength(float length)
    {
        coneLength = length;
       
        UpdateConeVisualsInEditor();
    }

#if UNITY_EDITOR
public Transform towerHead; // assign this in inspector

private void OnDrawGizmos()
{
    if (towerHead == null) return;

    SetConeLength(targeting != null ? targeting.range : coneLength);
    Gizmos.color = Application.isPlaying ? Color.red : Color.yellow;

    Vector3 forward = towerHead.forward; // use tower head rotation
    Vector3 startPos = firePoint.position;
    startPos.y = 0f;

    // Compute cone edge directions
    Quaternion leftRayRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
    Quaternion rightRayRot = Quaternion.Euler(0, coneAngle / 2f, 0);

    Vector3 leftDir = leftRayRot * forward;
    Vector3 rightDir = rightRayRot * forward;

    Vector3 leftEnd = startPos + leftDir * coneLength;
    Vector3 rightEnd = startPos + rightDir * coneLength;
    leftEnd.y = 0f;
    rightEnd.y = 0f;

    // Draw cone edges
    Gizmos.DrawLine(startPos, leftEnd);
    Gizmos.DrawLine(startPos, rightEnd);

    // Draw full circle at coneLength distance
    int circleSegments = 60;
    Vector3 prevPoint = startPos + (towerHead.forward * coneLength);
    prevPoint.y = 0f;

    for (int i = 1; i <= circleSegments; i++)
    {
        float angle = (360f / circleSegments) * i;
        Quaternion rot = Quaternion.Euler(0, angle, 0);
        Vector3 nextPoint = startPos + (rot * towerHead.forward) * coneLength;
        nextPoint.y = 0f;

        Gizmos.DrawLine(prevPoint, nextPoint);
        prevPoint = nextPoint;
    }
}
#endif
}
