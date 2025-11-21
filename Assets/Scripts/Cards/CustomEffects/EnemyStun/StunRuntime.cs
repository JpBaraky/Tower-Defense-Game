using UnityEngine;

public class StunRuntime : MonoBehaviour
{
    [Header("Stun Visual")]
    [SerializeField] private GameObject stunPrefab;   // Assign your particle prefab here
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.2f, 0f);

    private GameObject stunInstance;
    private Transform stunAnchor;

    private float stunTimer;
    private bool stunned;

    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        stunPrefab = Resources.Load<GameObject>("StunEffect");

    }

    public void StartStun(float duration)
    {
             stunPrefab = Resources.Load<GameObject>("StunEffect");
        stunned = true;
        stunTimer = duration;

        if (stunInstance == null)
            SpawnEffect();

        ToggleEnemy(false);
    }

    private void SpawnEffect()
    {
        if (stunPrefab == null)
        {
            Debug.LogWarning("StunRuntime: Missing stunPrefab on " + name);
            return;
        }

        // Anchor above head
        stunAnchor = new GameObject("StunAnchor").transform;
        stunAnchor.SetParent(transform, worldPositionStays: false);
        stunAnchor.localPosition = offset;

        // Instantiate effect
        stunInstance = Instantiate(stunPrefab, stunAnchor);

        // Match enemy scale
        //stunInstance.transform.localScale = transform.lossyScale;

        // Reset rotation (important for circular particles)
        stunInstance.transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!stunned) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            stunned = false;
            ToggleEnemy(true);

            if (stunInstance != null) Destroy(stunInstance);
            if (stunAnchor != null) Destroy(stunAnchor.gameObject);

            Destroy(this);
        }
    }

    private void ToggleEnemy(bool value)
    {
        var mover = GetComponent<EnemyFollowPath>();
        if (mover) mover.enabled = value;
    }
}
