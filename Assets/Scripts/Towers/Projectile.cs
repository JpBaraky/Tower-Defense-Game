using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private Vector3 lastKnownPosition;
    private float speed;
    private float damage;
    private bool targetLost;

    public void Initialize(Transform target, float damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        lastKnownPosition = target.position;
        targetLost = false;
    }

    void Update()
    {
        if (target == null && !targetLost)
            targetLost = true;

        // Use target position if it's still alive, otherwise keep moving toward the last known position
        Vector3 destination = targetLost ? lastKnownPosition : target.position;
        Vector3 dir = destination - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    void HitTarget()
    {
        if (!targetLost)
        {
            Enemy e = target.GetComponent<Enemy>();
            if (e != null)
                e.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
