using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float damage;

    public void Initialize(Transform target, float damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        Enemy e = target.GetComponent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
