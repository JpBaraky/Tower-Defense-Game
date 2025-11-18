using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;   // add this

public class TowerClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask towerLayer;

    void Update()
    {
  

foreach (var col in FindObjectsOfType<Collider>())
{
    Debug.Log(col.name + " layer: " + LayerMask.LayerToName(col.gameObject.layer));
}
foreach (var c in FindObjectsOfType<Collider>())
{
    if (c.isTrigger)
        Debug.Log("Trigger: " + c.name);
}
foreach (var col in FindObjectsOfType<Collider>())
{
    var rb = col.attachedRigidbody;
    if (rb == null)
        Debug.Log("NO RB: " + col.name);
}
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