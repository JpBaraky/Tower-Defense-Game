using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TowerTargeting))]
public class AoEPulseTower : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseRadius = 5f;
    public float pulseDamage = 50f;
    public float pulseInterval = 3f;
    public float pulseDuration = 1f; // time for shockwave to expand

    public ParticleSystem pulseEffect;

    private TowerTargeting targeting;
    private float pulseTimer;

    // For Gizmo visualization
    [SerializeField, HideInInspector]
    private float gizmoCurrentRadius = 1f;

    private void Awake()
    {
        targeting = GetComponent<TowerTargeting>();
    }

    private void Update()
    {
        // Update particle visuals when editing
        if (!Application.isPlaying)
        {
            UpdatePulseVisualsInEditor();
            return;
        }

        pulseTimer += Time.deltaTime;

        // Only trigger pulse if there are enemies nearby
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        bool hasNearbyEnemy = false;
        Vector3 origin = transform.position;
        foreach (Enemy e in enemies)
        {
            if (e == null || !e.isActiveAndEnabled) continue;
            if (Vector3.Distance(e.transform.position, origin) <= pulseRadius)
            {
                hasNearbyEnemy = true;
                break;
            }
        }

        if (pulseTimer >= pulseInterval && hasNearbyEnemy)
        {
            pulseTimer = 0f;
            StartCoroutine(TriggerShockwave());
        }
    }

    private IEnumerator TriggerShockwave()
    {
        Vector3 origin = transform.position;
        float elapsed = 0f;
        float currentRadius = 0f;

        // Play pulse particle if assigned
        if (pulseEffect != null)
        {
            pulseEffect.Play();
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            currentRadius = Mathf.Lerp(0f, pulseRadius, elapsed / pulseDuration);

            // Update gizmo radius for visualization (runtime + editor)
            gizmoCurrentRadius = currentRadius;

            foreach (Enemy e in enemies)
            {
                if (e == null || !e.isActiveAndEnabled || damagedEnemies.Contains(e)) continue;
                float dist = Vector3.Distance(e.transform.position, origin);
                if (dist <= currentRadius)
                {
                    e.TakeDamage(pulseDamage);
                    damagedEnemies.Add(e);
                }
            }

            yield return null;
        }

        gizmoCurrentRadius = 0f; // reset after shockwave
    }

    private void UpdatePulseVisualsInEditor()
    {
        if (pulseEffect != null)
        {
            var main = pulseEffect.main;

            // Adjust particle lifetime and speed based on radius (like PoisonSprayerTower)
            float startLifetime = Mathf.Sqrt(pulseRadius);
            float startSpeed = (pulseRadius * 5f) / startLifetime;

            main.startSpeed = startSpeed;
            main.startLifetime = startLifetime / 5f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Always draw cyan circle for tower range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pulseRadius);

        // Draw orange expanding shockwave both in editor and play mode
        if (gizmoCurrentRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, gizmoCurrentRadius);
        }
    }
#endif
}
