using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTableSO", menuName = "Mini Game/Avoid Monster/Item Table")]
public class AvoidItemTable : ScriptableObject
{
    [SerializeField] private List<AvoidItemData> Items = new List<AvoidItemData>();
    public List<AvoidItemData> GetItems() => Items;
    private List<AvoidItemData> _spawnItems = new List<AvoidItemData>();
    private Dictionary<int, AvoidItemData> _itemDictionary = new Dictionary<int, AvoidItemData>();
    
    public GameObject ItemPrefab;
    
    public void InitializeRuntimeItems()
    {
        _spawnItems.Clear();
        foreach (var item in Items)
        {
            _itemDictionary.Add(item.ItemID, item);
            if(!item.canSpawn) continue;
            
            _spawnItems.Add(item);
        }
    }
    
    public AvoidItemData GetRandomItem()
    {
        if (_spawnItems.Count == 0)
            return null;
        
        int randomIndex = Random.Range(0, _spawnItems.Count);
        AvoidItemData selectedItem = _spawnItems[randomIndex];
        _spawnItems.RemoveAt(randomIndex);
        
        return selectedItem;
    }
    
    public AvoidItemData GetItemById(int itemId)
    {
        if (_itemDictionary.TryGetValue(itemId, out var item))
            return item;
        
        return null;
    }
    
    public int GetSpawnItemCount() => _spawnItems.Count;
}
