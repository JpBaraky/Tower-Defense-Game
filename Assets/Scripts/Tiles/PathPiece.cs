using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents one hexagonal path segment prefab.
/// Each piece can have one or more entrance/exit points,
/// which the generator uses to connect paths.
/// </summary>
public class PathPiece : MonoBehaviour
{
    [Header("Connections")]
    // Points where enemies can enter the tile
    public List<Transform> entrances = new List<Transform>();

    // Points where enemies can leave the tile
    public List<Transform> exits = new List<Transform>();

    [Header("Visuals (optional)")]
    public GameObject pathVisual; // optional visual element

    private void OnDrawGizmos()
    {
        // Show entrances in green, exits in red
        Gizmos.color = Color.green;
        foreach (var e in entrances)
            if (e != null)
                Gizmos.DrawSphere(e.position, 0.05f);

        Gizmos.color = Color.red;
        foreach (var e in exits)
            if (e != null)
                Gizmos.DrawSphere(e.position, 0.05f);
    }
}
