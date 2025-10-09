using UnityEngine;

public class EnemyFacePath : MonoBehaviour
{
    private EnemyFollowPath enemyFollowPath;
    private Transform targetNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      enemyFollowPath = GetComponentInParent<EnemyFollowPath>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyFollowPath.nextNode == null)
        {
            return;
        }
        targetNode = enemyFollowPath.nextNode;
        Vector3 direction = targetNode.transform.position - transform.position;

        // Ignore vertical difference to rotate only around Y
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // Rotate to face the target on Y axis
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
    }

