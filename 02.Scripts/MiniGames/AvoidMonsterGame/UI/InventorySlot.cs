using TMPro;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _itemNameText;

    public AvoidItemData ItemData { get; private set; }

    public void Initialize(AvoidItemData itemData)
    {
        ItemData = itemData;
        _itemNameText.text = itemData.ItemName;
    }
}
