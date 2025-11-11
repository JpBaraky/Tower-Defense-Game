using UnityEngine;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;
    public TowerUI ui;
    public GameObject coinEffectPrefab;
    public AudioClip sellSound;

    private TowerSelectable selectedTower;
    private GameObject rangeIndicator;
    private TowerPlacement placement; // your economy system reference

    [Header("Upgrade Settings")]
    public int baseUpgradeCost = 50;
    public float upgradeCostMultiplier = 1.5f;
    [HideInInspector] public int upgradeCost;

    [SerializeField] private int circleSegments = 64;

    void Awake() => Instance = this;

    void Start()
    {
        placement = FindFirstObjectByType<TowerPlacement>();
        if (placement == null)
            Debug.LogError("TowerPlacement not found in scene. TowerSelectionManager requires it.");
    }

    public void SelectTower(TowerSelectable tower)
    {
        selectedTower = tower;
        upgradeCost = Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, selectedTower.level - 1));
      
        ui.upgradeCost = upgradeCost;
        ui.ShowTowerInfo(tower);
        ShowRangeIndicator(tower);
    }

    void ShowRangeIndicator(TowerSelectable tower)
    {
        if (rangeIndicator != null)
            Destroy(rangeIndicator);

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

        float angleStep = 360f / circleSegments;
        Vector3[] points = new Vector3[circleSegments];
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            points[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * tower.range;
        }

        line.SetPositions(points);
        rangeIndicator.transform.position = tower.transform.position + Vector3.up * 0.01f;
    }

    public void UpgradeTower()
    {
        if (selectedTower == null || placement == null) return;

       // upgradeCost = upgradeCost = Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, selectedTower.level));

      
        if (placement.currentGold < upgradeCost)
        {
            Debug.Log("Not enough gold to upgrade.");
            return;
        }

        placement.currentGold -= upgradeCost;
          
        Debug.Log($"Upgraded {selectedTower.towerName} to level {selectedTower.level}. Cost: {upgradeCost}. Remaining gold: {placement.currentGold}");


        selectedTower.level++;
        selectedTower.damage *= 1.25f;
        selectedTower.range *= 1.1f;
        selectedTower.targeting.damage = selectedTower.damage;
        selectedTower.targeting.range = selectedTower.range;
        selectedTower.targeting.level = selectedTower.level;
        upgradeCost = Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, selectedTower.level - 1));
ui.upgradeCost = upgradeCost;

        
        ui.ShowTowerInfo(selectedTower);
        ShowRangeIndicator(selectedTower);
    }

   public void SellTower()
{
    if (selectedTower == null || placement == null) return;

    TowerPrice priceComponent = selectedTower.GetComponent<TowerPrice>();
        if (priceComponent == null)
        {
            Debug.LogWarning("Tower has no TowerPrice component, cannot calculate refund.");
            return;
        }
         var effect = Instantiate(coinEffectPrefab, selectedTower.transform.position, Quaternion.identity);
Destroy(effect, 1.5f); // destroys after 1.5 seconds
    AudioSource.PlayClipAtPoint(sellSound, Camera.main.transform.position);

    // Refund
    int refund = Mathf.RoundToInt(priceComponent.price * placement.towerSellMultiplier);
    placement.currentGold += refund;

    // Free the occupied tile in the placement dictionary
    placement.FreeCellAtPosition(selectedTower.transform.position);

    Debug.Log($"Sold tower for {refund} gold. Current gold: {placement.currentGold}");

    Destroy(selectedTower.gameObject);
    selectedTower = null;
  
    if (rangeIndicator != null) Destroy(rangeIndicator);
    ui.Hide();
}
}
