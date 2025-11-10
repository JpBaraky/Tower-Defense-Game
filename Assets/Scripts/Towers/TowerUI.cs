using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text nameText, levelText, dmgText, rangeText;
    public Button upgradeButton;

    void Start()
    {
        panel.SetActive(false);
        upgradeButton.onClick.AddListener(() => TowerSelectionManager.Instance.UpgradeTower());
    }

    public void ShowTowerInfo(TowerSelectable tower)
    {
        panel.SetActive(true);
        nameText.text = tower.towerName;
        levelText.text = "Level: " + tower.level;
        dmgText.text = "Damage: " + tower.damage.ToString("F1");
        rangeText.text = "Range: " + tower.range.ToString("F1");
    }
}
