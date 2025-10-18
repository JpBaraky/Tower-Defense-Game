using UnityEngine;
using System.Collections.Generic;

public class PathPiece : MonoBehaviour
{
    [HideInInspector] public List<Transform> exits = new();

    public void Awake()
    {
        exits.Clear();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Exit")) // mark your exit objects with "Exit" tag
                exits.Add(child);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var exit in exits)
            Gizmos.DrawSphere(exit.position, 0.1f);
    }
}