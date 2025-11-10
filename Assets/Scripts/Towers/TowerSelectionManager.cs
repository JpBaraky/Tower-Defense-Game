using UnityEngine;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;
    public TowerUI ui;
    private TowerSelectable selectedTower;
    private GameObject rangeIndicator;
    [SerializeField] private int circleSegments = 64; // smoothness

    void Awake() => Instance = this;

    public void SelectTower(TowerSelectable tower)
    {
        selectedTower = tower;
        ui.ShowTowerInfo(tower);
        ShowRangeIndicator(tower);
    }

    void ShowRangeIndicator(TowerSelectable tower)
    {
        if (rangeIndicator != null) Destroy(rangeIndicator);

        rangeIndicator = new GameObject("RangeIndicator");
        var line = rangeIndicator.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = circleSegments;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0f, 1f, 0f, 0.6f);
        line.endColor = new Color(0f, 1f, 0f, 0.6f);

        // generate circle points
        float angleStep = 360f / circleSegments;
        Vector3[] points = new Vector3[circleSegments];
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            points[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * tower.range;
        }
        line.SetPositions(points);

        // position under tower
        rangeIndicator.transform.position = tower.transform.position + Vector3.up * 0.01f; // small offset to avoid z-fighting
    }

    public void UpgradeTower()
    {
        if (selectedTower == null) return;

        selectedTower.level++;
        selectedTower.damage *= 1.25f;
        selectedTower.range *= 1.1f;

        ui.ShowTowerInfo(selectedTower);
        ShowRangeIndicator(selectedTower);
    }
}
