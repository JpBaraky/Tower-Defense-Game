using UnityEngine;

public class ButtonBuyTower : MonoBehaviour
{

    private TowerPlacement towerPlacement;

    public GameObject towerPrefab;
    void Start()
    {
        towerPlacement = FindAnyObjectByType<TowerPlacement>();
    }



    public void ButtonTower()
    {
        towerPlacement.towerPrefab = towerPrefab;
      
        towerPlacement.CanPlaceTower();

    }

}
