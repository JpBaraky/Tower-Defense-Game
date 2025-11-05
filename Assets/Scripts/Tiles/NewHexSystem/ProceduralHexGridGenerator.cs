using System.Collections.Generic;
using UnityEngine;

public class HexSubTileGenerator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject smallHexPrefab;
    public float smallHexRadius = 0.33f; // 1/3 of main hex
    public float heightStep = 0.2f;
    public int Numbersteps = 4; // 0, 0.2, 0.4, 0.6

    private List<SmallHex> smallHexes = new List<SmallHex>();

    void Start()
    {
        bool hasNeighbor = CheckAdjacentLargeTiles();
        if (hasNeighbor)
        {
            GenerateSmallHexes();
            AssignHeights();
        }
    }

    bool CheckAdjacentLargeTiles()
    {
        // Find all large tiles with this script
        // Find all large tiles with the HexSubTileGenerator script
        var allTiles = Object.FindObjectsByType<HexSubTileGenerator>(
            FindObjectsSortMode.None // you can also use SceneOrder or HierarchyOrder 
);

foreach (var tile in allTiles)
{
    if (tile == this) continue; // skip self
    float dist = Vector3.Distance(transform.position, tile.transform.position);
    if (dist > 0.01f && dist <= 1.05f)
        return true; // has neighbor
}

        foreach (var tile in allTiles)
        {
            if (tile == this) continue;
            float dist = Vector3.Distance(transform.position, tile.transform.position);
            // Consider adjacent if distance is roughly equal to tile size (adjust 1.05f if needed)
            if (dist > 0.01f && dist <= 1.05f)
                return true;
        }
        return false;
    }

    void GenerateSmallHexes()
    {
        smallHexes.Clear();

        // Center hex
        var centerHex = Instantiate(smallHexPrefab, transform.position, Quaternion.identity, transform);
        centerHex.transform.localScale = Vector3.one * smallHexRadius;
        smallHexes.Add(new SmallHex(centerHex));

        float offset = smallHexRadius * 0.866f;

        for (int i = 0; i < 6; i++)
        {
            float angleRad = Mathf.Deg2Rad * (60f * i);
            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)) * offset;
            Vector3 pos = transform.position + dir;

            var hexObj = Instantiate(smallHexPrefab, pos, Quaternion.identity, transform);
            hexObj.transform.localScale = Vector3.one * smallHexRadius;
            smallHexes.Add(new SmallHex(hexObj));
        }

        transform.rotation = Quaternion.identity;
    }

    void AssignHeights()
    {
        float[] possibleHeights = new float[Numbersteps];
        for (int i = 0; i < Numbersteps; i++)
            possibleHeights[i] = i * heightStep;

        foreach (var hex in smallHexes)
        {
            // Randomize height
            hex.height = possibleHeights[Random.Range(0, Numbersteps)];
            hex.step = Mathf.RoundToInt(hex.height / heightStep);

            StepHeight stepComp = hex.obj.GetComponent<StepHeight>();
            if (stepComp != null)
                stepComp.step = hex.step;

            Vector3 s = hex.obj.transform.localScale;
            s.y = hex.height + 0.13f;
            hex.obj.transform.localScale = s;
        }
    }

    class SmallHex
    {
        public GameObject obj;
        public float height;
        public int step;
        public SmallHex(GameObject o) { obj = o; }
    }
}
