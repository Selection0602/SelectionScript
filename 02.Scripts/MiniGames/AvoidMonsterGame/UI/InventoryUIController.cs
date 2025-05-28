using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUIController : BaseAvoidUIController
{
    [SerializeField] private GameObject _selectCursor; // 선택 커서
    [SerializeField] private InventorySlot[] _itemSlots; // 아이템 슬롯
    [SerializeField] private TextMeshProUGUI _itemDescription; // 아이템 설명
    
    [SerializeField] private Inventory _inventory;
    [SerializeField] private DialogueUIController _dialogueUIController;
    
    private List<AvoidItemData> _itemDataList; 
    private Dictionary<int, AvoidItemData> _itemDataDict; // 조합 확인용 딕셔너리
    
    private int _currentIndex = 0; // 현재 선택된 슬롯 인덱스
    private AvoidItemData _currentItemData;
    
    public event Action OnReturnMenu;
    
    private void OnEnable()
    {
        _selectCursor.SetActive(false);
        _itemDescription.text = string.Empty;
        
        foreach (var slot in _itemSlots)
            slot.gameObject.SetActive(false);
        
        _itemDataList = _inventory.GetItemsList();
        _itemDataDict = _inventory.GetItemsDictionary();
        
        if(_itemDataList is { Count: > 0 })
            InitializeItemSlots();
    }

    private void InitializeItemSlots()
    {
        for (int index = 0; index < _itemDataList.Count; index++)
        {
            _itemSlots[index].gameObject.SetActive(true);
            _itemSlots[index].Initialize(_itemDataList[index]);
        }

        UpdateCursorPosition();
        UpdateItemDescription();
    }
    
    private void UpdateCursorPosition()
    {
        if (_itemSlots.Length <= 0) return;
        _selectCursor.SetActive(true);
        _selectCursor.transform.position = _itemSlots[_currentIndex].transform.position;
    }
    
    private void UpdateItemDescription()
    {
        if (_itemSlots.Length <= 0) return;
        
        _currentItemData = _itemDataList[_currentIndex];
        _itemDescription.text = _itemDataList[_currentIndex].Description;
    }
    
    public override void OnMoveUp()
    {
        if (_currentIndex >= 3)
        {
            _currentIndex -= 3;
            UpdateCursorPosition();
            UpdateItemDescription();
        }
    }

    public override void OnMoveDown()
    {
        if (_currentIndex + 3 < _itemDataList.Count && _currentIndex + 3 < _itemDataList.Count)
        {
            _currentIndex += 3;
            UpdateCursorPosition();
            UpdateItemDescription();
        }
    }

    public override void OnMoveLeft()
    {
        if (_currentIndex % 3 > 0)
        {
            _currentIndex--;
            UpdateCursorPosition();
            UpdateItemDescription();
        }
    }

    public override void OnMoveRight()
    {
        if (_currentIndex % 3 < 2 && _currentIndex + 1 < _itemDataList.Count && _currentIndex + 1 < _itemDataList.Count)
        {
            _currentIndex++;
            UpdateCursorPosition();
            UpdateItemDescription();
        }
    }
    
    public override void OnSubmit()
    {
        if (!_currentItemData || _currentItemData.ItemID is not (0 or 1)) return;
        
        int itemId = _currentItemData.ItemID;
        int needItemId = itemId == 0 ? 1 : 0;

        if (!_itemDataDict.ContainsKey(needItemId)) return;

        if (_inventory.TryCombineItems(new List<int> { itemId, needItemId }, out var resultItemId))
        {
            Inventory inventory = _inventory;
            AvoidItemData resultItemData = _inventory.GetItemData(resultItemId);
            
            _dialogueUIController.StartDialogueWithChoices(new []{"손수건에 세제를 묻힐까?"},
                () => _dialogueUIController.StartDialogue(new []{"손수건에 세제를 묻혔다."}, () =>
                {
                    inventory.RemoveItem(itemId);
                    inventory.RemoveItem(needItemId);
                    inventory.AddItem(resultItemData);
                    _dialogueUIController.Hide();
                }, resultItemData.ItemSprite),
                ()=> _dialogueUIController.Hide());
            
            Hide();
        }
    }
    
    public override void OnCancel()
    {
        OnReturnMenu?.Invoke();
    }

    private void OnDisable()
    {
        _selectCursor.SetActive(false);
        _itemDescription.text = string.Empty;
        
        foreach (var slot in _itemSlots)
            slot.gameObject.SetActive(false);
        
        _currentIndex = 0;
        _currentItemData = null;
    }
}
