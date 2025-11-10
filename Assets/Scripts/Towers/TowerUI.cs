using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_Text nameText, levelText, dmgText, rangeText;
    public Button upgradeButton;
    public Button sellButton;
    public float upgradeCost;
    

    private TowerTargeting targeting;

    void Start()
    {
        if (panel == null)
            panel = gameObject;


        panel.SetActive(false);

        // Hook up buttons
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => TowerSelectionManager.Instance.UpgradeTower());
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(() => TowerSelectionManager.Instance.SellTower());
    }

    public void ShowTowerInfo(TowerSelectable tower)
    {
        if (tower == null) return;

        panel.SetActive(true);
        targeting = tower.GetComponent<TowerTargeting>();
        upgradeButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade: {upgradeCost} Gold";

        nameText.text = tower.towerName;
        levelText.text = $"Level: {tower.level}";
        dmgText.text = $"Damage: {targeting.damage:F1}";
        rangeText.text = $"Range: {targeting.range:F1}";
        
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
