using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

[ExecuteAlways]
public class DoorFaceTileNodeY : MonoBehaviour
{
    void Update()
    {
        // Find the first TileNode with name "PathNode(Clone)" and number 0
        TileNode targetNode = Resources
            .FindObjectsOfTypeAll<TileNode>()
            .FirstOrDefault(node => node.gameObject.name == "PathNode(Clone)" && node.number == 0);

        if (targetNode == null)
            return;

        // Direction from this object to the target
        Vector3 direction = targetNode.transform.position - transform.position;

        // Ignore vertical difference to rotate only around Y
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // Rotate to face the target on Y axis
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        TileNode targetNode = Resources
            .FindObjectsOfTypeAll<TileNode>()
            .FirstOrDefault(node => node.gameObject.name == "PathNode(Clone)" && node.number == 0);

        if (targetNode != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetNode.transform.position);
        }
    }
#endif
}
