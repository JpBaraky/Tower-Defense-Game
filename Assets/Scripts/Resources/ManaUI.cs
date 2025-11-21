using UnityEngine;
using TMPro;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text manaText;

    void Start()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnManaChanged += OnManaChanged;

        // initialize display
        if (ResourceManager.Instance != null)
            OnManaChanged(ResourceManager.Instance.CurrentMana, ResourceManager.Instance.MaxMana);
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnManaChanged -= OnManaChanged;
    }

    private void OnManaChanged(int current, int max)
    {
        if (manaText != null)
            manaText.text = $"Mana: {current}/{max}";
    }
}
