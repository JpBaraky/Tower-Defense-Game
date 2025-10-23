using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class PoisonSprayerTower : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(20, 90)] public float coneAngle = 45f;
    [Min(0f)] public float coneLength = 1f;
    public Transform firePoint;

    [Header("Damage Settings")]
    public float towerDamage = 20f;
    public bool applyPoison = true;
    public float poisonDamage = 5f;
    public float poisonDuration = 2f;
    public float damageInterval = 0.1f;
    public float poisonVFXFadeTime = 0.5f; // Duration for VFX to fade

    [Header("Visuals")]
    public ParticleSystem poisonEffect;
    public GameObject poisonEffectPrefab;

    private TowerTargeting targeting;
    private float damageTimer;
    private Coroutine stopPoisonCoroutine;


    private class PoisonInfo
    {
        public float timeLeft;
        public GameObject poisonVFX;
        public Coroutine fadeCoroutine;
    }

    private readonly Dictionary<Enemy, PoisonInfo> poisonedEnemies = new();

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
        if (stopPoisonCoroutine == null)
            stopPoisonCoroutine = StartCoroutine(DelayedStopPoison(0.1f));
    }
    else
    {
        // Cancel delayed stop if target came back
        if (stopPoisonCoroutine != null)
        {
            StopCoroutine(stopPoisonCoroutine);
            stopPoisonCoroutine = null;
        }

        // Play poison effect if not already playing
        if (poisonEffect != null && !poisonEffect.isPlaying)
            poisonEffect.Play();

        // Apply damage over time
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyConeDamage();
            damageTimer = 0f;
        }
    }
        // Always update poisoned enemies to remove expired VFX
        UpdatePoisonedEnemies();
    }
    private IEnumerator DelayedStopPoison(float delay)
{
    yield return new WaitForSeconds(delay);
    StopPoison();
    stopPoisonCoroutine = null;
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

            if (applyPoison)
            {
                if (poisonedEnemies.TryGetValue(e, out var info))
                {
                    info.timeLeft = poisonDuration; // Refresh poison duration
                }
                else
                {
                    GameObject fx = null;
                    if (poisonEffectPrefab)
                    {
                        fx = Instantiate(poisonEffectPrefab, e.transform);
                        fx.transform.localPosition = Vector3.zero;
                    }

                    poisonedEnemies[e] = new PoisonInfo
                    {
                        timeLeft = poisonDuration,
                        poisonVFX = fx,
                        fadeCoroutine = null
                    };
                }
            }
        }
    }

    private void UpdatePoisonedEnemies()
    {
        if (poisonedEnemies.Count == 0) return;

        List<Enemy> toRemove = new();

        foreach (var kvp in poisonedEnemies)
        {
            Enemy e = kvp.Key;
            PoisonInfo info = kvp.Value;

            bool removeEnemy = false;

            if (e == null || !e.isActiveAndEnabled)
            {
                removeEnemy = true;
            }
            else
            {
                e.TakeDamage(poisonDamage * Time.deltaTime);
                info.timeLeft -= Time.deltaTime;

                if (info.timeLeft <= 0)
                    removeEnemy = true;
            }

            if (removeEnemy)
            {
                if (info.poisonVFX != null)
                {
                    if (info.fadeCoroutine != null)
                        StopCoroutine(info.fadeCoroutine);

                    info.fadeCoroutine = StartCoroutine(FadeAndDestroy(info.poisonVFX, poisonVFXFadeTime));
                    info.poisonVFX = null;
                }

                toRemove.Add(e);
            }
        }

        foreach (var e in toRemove)
            poisonedEnemies.Remove(e);

        if (poisonedEnemies.Count == 0 && poisonEffect != null && poisonEffect.isPlaying)
            poisonEffect.Stop();
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

    private void StopPoison()
    {
        if (poisonEffect != null && poisonEffect.isPlaying)
            poisonEffect.Stop();
    }

    private void UpdateConeVisualsInEditor()
    {
        if (poisonEffect != null)
        {
            var shape = poisonEffect.shape;
            shape.angle = coneAngle / 2;
            
             var main = poisonEffect.main; 

float startLifetime = Mathf.Sqrt(coneLength);              // grows slower than linear
float startSpeed = (coneLength * 10f) / startLifetime;   


// Apply to particle system
main.startSpeed = startSpeed;
main.startLifetime = startLifetime;

//adjust emission to maintain density
var emission = poisonEffect.emission;
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
