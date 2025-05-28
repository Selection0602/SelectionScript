using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private AvoidItemTable _itemTable; // 아이템 테이블
    [SerializeField] private List<ItemRecipe> _itemRecipes; // 아이템 조합 레시피 목록

    private Dictionary<int, AvoidItemData> _itemDict = new Dictionary<int, AvoidItemData>(); // 보유 아이템 목록
    private Dictionary<string, int> _recipeDict = new Dictionary<string, int>(); // 조합 레시피 딕셔너리

    private List<int> _monsterSpawnItemList = new List<int>() { 2, 3, 4 }; // 몬스터 스폰 아이템 목록
    public event Action OnSpawnItemAdded;

#if UNITY_EDITOR
    public bool IsDebug = false;
#endif

    private void Awake()
    {
        InitializeRecipes();

#if UNITY_EDITOR
        if (IsDebug)
            InitializeItems();
#endif
    }

    private void InitializeItems()
    {
        _itemDict.Clear();
        foreach (var item in _itemTable.GetItems())
        {
            if (item.ItemID != 5)
                _itemDict.Add(item.ItemID, item);
        }
    }

    private void InitializeRecipes()
    {
        _recipeDict.Clear();

        foreach (var recipe in _itemRecipes)
        {
            List<int> sortedIds = new List<int>(recipe.IngredientIdList);
            sortedIds.Sort();

            string key = string.Join(",", sortedIds);

            _recipeDict[key] = recipe.ResultId;
        }
    }

    public void AddItem(AvoidItemData item)
    {
        if (!item) return;

        _itemDict.Add(item.ItemID, item);

        if (_monsterSpawnItemList.Contains(item.ItemID))
            OnSpawnItemAdded?.Invoke();
    }

    public bool TryCombineItems(List<int> itemIds, out int resultItemId)
    {
        resultItemId = -1;

        List<int> sortedIds = new List<int>(itemIds);
        sortedIds.Sort();

        string key = string.Join(",", sortedIds);

        return _recipeDict.TryGetValue(key, out resultItemId);
    }

    public AvoidItemData GetItemData(int itemId)
    {
        if (_itemTable.GetItemById(itemId) is { } itemData)
            return itemData;

        return null;
    }

    public bool HasItem(int itemId) => _itemDict.ContainsKey(itemId);
    public void RemoveItem(int itemId) => _itemDict.Remove(itemId);
    public List<AvoidItemData> GetItemsList() => _itemDict.Values.ToList();
    public Dictionary<int, AvoidItemData> GetItemsDictionary() => _itemDict;
}
