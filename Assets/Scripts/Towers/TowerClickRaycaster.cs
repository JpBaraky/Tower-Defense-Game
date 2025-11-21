using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;   // add this

public class TowerClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask towerLayer;

    void Update()
    {
  


 if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, towerLayer))
            {
                Debug.Log("Hit: " + hit.collider.name);
                var tower = hit.collider.GetComponentInParent<TowerSelectable>();
                if (tower != null)
                    TowerSelectionManager.Instance.SelectTower(tower);
            }
        }
    }
}