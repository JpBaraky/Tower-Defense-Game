using UnityEngine;
using UnityEngine.Animations;

public class EnemyDamageTower : MonoBehaviour
{
    public int damageAmount = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            CastleHealth castle = other.GetComponent<CastleHealth>();
            if (castle != null)
            {
                castle.TakeDamage(damageAmount);
            }

            Destroy(this.transform.parent.parent.gameObject); 
        }
    }
}