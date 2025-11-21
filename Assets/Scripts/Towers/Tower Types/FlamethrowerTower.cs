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
    public float FireVFXFadeTime = 0.5f;

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
            if (stopFireCoroutine == null)
                stopFireCoroutine = StartCoroutine(DelayedStopFire(0.1f));
        }
        else
        {
            if (stopFireCoroutine != null)
            {
                StopCoroutine(stopFireCoroutine);
                stopFireCoroutine = null;
            }

            if (FireEffect != null && !FireEffect.isPlaying)
                FireEffect.Play();

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                ApplyConeDamage();
                damageTimer = 0f;
            }
        }

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
        Vector3 origin = transform.position;
        Vector3 forward = towerHead != null ? towerHead.forward : transform.forward;

        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.isActiveAndEnabled)
                continue;

            Vector3 dir = e.transform.position - origin;
            Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
            float dist = flatDir.magnitude;
            if (dist > coneLength) continue;

            float angle = Vector3.Angle(flatForward, flatDir);
            if (angle > coneAngle / 2f) continue;

            e.TakeDamage(towerDamage * (1 + targeting.heightStep / 10f) * damageInterval);

            if (applyFire)
            {
                if (FireedEnemies.TryGetValue(e, out var info))
                {
                    info.timeLeft = FireDuration * (1 + targeting.heightStep / 10f);
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
                        timeLeft = FireDuration * (1 + targeting.heightStep / 10f),
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
            float startLifetime = Mathf.Sqrt(coneLength);
            float startSpeed = (coneLength * 10f) / startLifetime;

            main.startSpeed = startSpeed;
            main.startLifetime = startLifetime;

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
    public Transform towerHead;

    private void OnDrawGizmos()
    {
        if (towerHead == null || firePoint == null) return;

        SetConeLength(targeting != null ? targeting.range : coneLength);
        Gizmos.color = Application.isPlaying ? Color.red : Color.yellow;

        Vector3 forward = towerHead.forward;
        Vector3 startPos = firePoint.position;

        Quaternion leftRayRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRayRot = Quaternion.Euler(0, coneAngle / 2f, 0);

        Vector3 leftDir = leftRayRot * forward;
        Vector3 rightDir = rightRayRot * forward;

        Vector3 leftEnd = startPos + leftDir * coneLength;
        Vector3 rightEnd = startPos + rightDir * coneLength;
        leftEnd.y = startPos.y;
        rightEnd.y = startPos.y;

        Gizmos.DrawLine(startPos, leftEnd);
        Gizmos.DrawLine(startPos, rightEnd);

        int circleSegments = 60;
        Vector3 prevPoint = startPos + (towerHead.forward * coneLength);
        prevPoint.y = startPos.y;

        for (int i = 1; i <= circleSegments; i++)
        {
            float angle = (360f / circleSegments) * i;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 nextPoint = startPos + (rot * towerHead.forward) * coneLength;
            nextPoint.y = startPos.y;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
