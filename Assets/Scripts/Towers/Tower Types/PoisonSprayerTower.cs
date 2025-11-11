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
    public float poisonVFXFadeTime = 0.5f;

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
        if (targeting.isPreview)
            return;

        if (!Application.isPlaying)
        {
            UpdateConeVisualsInEditor();
            return;
        }

        Enemy target = targeting.currentTarget;

        if (target == null)
        {
            if (stopPoisonCoroutine == null)
                stopPoisonCoroutine = StartCoroutine(DelayedStopPoison(0.15f));
        }
        else
        {
            if (stopPoisonCoroutine != null)
            {
                StopCoroutine(stopPoisonCoroutine);
                stopPoisonCoroutine = null;
            }

            if (poisonEffect != null && !poisonEffect.isPlaying)
                poisonEffect.Play();

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                ApplyConeDamage();
                damageTimer = 0f;
            }
        }

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
        if (targeting == null) return;

        Vector3 forward = towerHead != null ? towerHead.forward : transform.forward;
        Vector3 origin = firePoint ? firePoint.position : transform.position;
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.isActiveAndEnabled) continue;

            Vector3 dir = e.transform.position - origin;
            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;
            Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
            float dist = flatDir.magnitude;
            if (dist > coneLength) continue;

            float angle = Vector3.Angle(flatForward, flatDir);
            if (angle > coneAngle * 0.5f) continue;

            e.TakeDamage(towerDamage * (1 + targeting.heightStep / 10f) * damageInterval);

            if (!applyPoison) continue;

            if (poisonedEnemies.TryGetValue(e, out var info))
            {
                info.timeLeft = poisonDuration * (1 + targeting.heightStep / 10f);
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
                    timeLeft = poisonDuration * (1 + targeting.heightStep / 10f),
                    poisonVFX = fx,
                    fadeCoroutine = null
                };
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

            bool expired = false;

            if (e == null || !e.isActiveAndEnabled)
            {
                expired = true;
            }
            else
            {
                e.TakeDamage(poisonDamage * Time.deltaTime);
                info.timeLeft -= Time.deltaTime;

                if (info.timeLeft <= 0)
                    expired = true;
            }

            if (expired)
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
            var emission = ps.emission;
            float startRate = emission.rateOverTime.constant;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                emission.rateOverTime = Mathf.Lerp(startRate, 0f, t);
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
        if (poisonEffect == null) return;

        var shape = poisonEffect.shape;
        shape.angle = coneAngle * 0.5f;

        var main = poisonEffect.main;
        float lifetime = Mathf.Max(0.5f, Mathf.Sqrt(coneLength));
        float speed = Mathf.Max(0.1f, (coneLength * 10f) / lifetime);

        main.startSpeed = speed;
        main.startLifetime = lifetime;

        var emission = poisonEffect.emission;
        emission.rateOverTime = coneLength * 25f;
    }

    public void SetConeLength(float length)
    {
        coneLength = Mathf.Max(0f, length);
        UpdateConeVisualsInEditor();
    }

#if UNITY_EDITOR
    public Transform towerHead;

    private void OnDrawGizmos()
    {
        if (firePoint == null || (targeting == null && towerHead == null)) return;

        float range = targeting != null ? targeting.range : coneLength;
        SetConeLength(range);

        Gizmos.color = Application.isPlaying ? Color.green : Color.yellow;

        Vector3 forward = towerHead ? towerHead.forward : transform.forward;
        Vector3 startPos = firePoint.position;

        Quaternion leftRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, coneAngle / 2f, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(startPos, startPos + leftDir * range);
        Gizmos.DrawLine(startPos, startPos + rightDir * range);

        int segs = 60;
        Vector3 prev = startPos + forward * range;

        for (int i = 1; i <= segs; i++)
        {
            float a = (360f / segs) * i;
            Quaternion rot = Quaternion.Euler(0, a, 0);
            Vector3 next = startPos + (rot * forward) * range;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
