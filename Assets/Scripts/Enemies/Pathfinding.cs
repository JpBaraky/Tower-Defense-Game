using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private List<TileNode> nodes = new();

    IEnumerator Start()
    {
        // wait one frame so spawner (Start) runs first
        yield return null;
        RefreshNodes();
    }

    public void RefreshNodes()
    {
        nodes = FindObjectsByType<TileNode>(FindObjectsSortMode.None).Where(n => n != null).ToList();
       
    }

    TileNode GetClosestNode(Vector3 pos) =>
        nodes.OrderBy(n => (n.transform.position - pos).sqrMagnitude).FirstOrDefault();

    List<TileNode> GetNeighbors(TileNode node, float neighborDistance = 1.1f)
    {
        float sq = neighborDistance * neighborDistance;
        return nodes.Where(n => n.walkable && n != node && (n.transform.position - node.transform.position).sqrMagnitude <= sq).ToList();
    }

    float GetDistance(TileNode a, TileNode b) => Vector3.Distance(a.transform.position, b.transform.position);

    public List<TileNode> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (nodes == null || nodes.Count == 0) RefreshNodes();
        if (nodes.Count == 0) { Debug.LogWarning("Pathfinding: no nodes available"); return null; }

        TileNode start = GetClosestNode(startPos);
        TileNode target = GetClosestNode(targetPos);
        if (start == null || target == null) { Debug.LogWarning("Pathfinding: start or target node is null"); return null; }
        if (!start.walkable || !target.walkable) { Debug.LogWarning("Pathfinding: start/target not walkable"); return null; }

        var openSet = new List<TileNode> { start };
        var cameFrom = new Dictionary<TileNode, TileNode>();
        var gScore = nodes.ToDictionary(n => n, n => float.PositiveInfinity);
        var fScore = nodes.ToDictionary(n => n, n => float.PositiveInfinity);

        gScore[start] = 0;
        fScore[start] = GetDistance(start, target);

        while (openSet.Count > 0)
        {
            TileNode current = openSet.OrderBy(n => fScore[n]).First();
            if (current == target) return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            foreach (var neighbor in GetNeighbors(current))
            {
                float tentativeG = gScore[current] + GetDistance(current, neighbor);
                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = gScore[neighbor] + GetDistance(neighbor, target);
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("Pathfinding: path not found");
        return null;
    }

    List<TileNode> ReconstructPath(Dictionary<TileNode, TileNode> cameFrom, TileNode current)
    {
        var path = new List<TileNode> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}