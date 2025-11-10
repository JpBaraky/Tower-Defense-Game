using UnityEngine;
public class TowerSelectable : MonoBehaviour
{
    public string towerName;
    public float range;
    public float level;
    public float damage;
    [HideInInspector]
    public TowerTargeting targeting;

    void Start()
    {
        targeting = GetComponent<TowerTargeting>();
        range = targeting.range;
        damage = targeting.damage;
        level = targeting.level;
        Debug.Log(level);
    }

    void Update()
    {
        towerName = gameObject.name;
        UpdateStats();

    }
    void OnMouseDown()
    {
        Debug.Log("Tower " + towerName + " selected.");
        TowerSelectionManager.Instance.SelectTower(this);
    }
    public void UpdateStats()
    {
         range = targeting.range;
        damage = targeting.damage;
        level = targeting.level;
    }
    
}