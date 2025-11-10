using UnityEngine;
public class TowerSelectable : MonoBehaviour
{
    public string towerName;
    public float range;
    public int level;
    public float damage;

    void OnMouseDown()
    {
        Debug.Log("Tower " + towerName + " selected.");
        TowerSelectionManager.Instance.SelectTower(this);
    }
}