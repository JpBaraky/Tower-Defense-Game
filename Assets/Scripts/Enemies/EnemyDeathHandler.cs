using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    public WaveSpawner spawner;

    private void OnDestroy()
    {
        if (spawner != null && Application.isPlaying)
        {
            spawner.OnEnemyDeath();
        }
    }
}
