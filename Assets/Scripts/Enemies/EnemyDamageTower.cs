using UnityEngine;

public class EnemyDamageTower : MonoBehaviour
{
    public int damageAmount = 1;
    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Castle"))
    {
        CastleHealth castle = other.GetComponent<CastleHealth>();
        if (castle != null)
            castle.TakeDamage(damageAmount);

        Destroy(gameObject); // or play death animation
    }
}
}
