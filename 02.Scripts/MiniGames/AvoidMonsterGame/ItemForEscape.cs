using System;
using UnityEngine;

public interface IInteractable
{
    void ProcessInteraction(AMPlayer player, Action onInteractComplete = null);
    void OnInteractionComplete();
    
    event Action OnInteractComplete;    // 상호작용 완료 시 호출
    bool IsInteractable { get; }      // 상호작용 가능 여부
    Direction InteractableDirection { get; } // 상호작용 방향
}

public class ItemForEscape : MonoBehaviour, IInteractable
{
    public AvoidItemData ItemData;
    
    [SerializeField] private AvoidItemDataEventChannel _itemDataEventChannel;
    public Sprite ItemSprite; // 아이템 이미지

    public bool IsInteractable { get; private set; } = true;

    [SerializeField] private Direction _interactableDirection = Direction.None;
    public Direction InteractableDirection => _interactableDirection;
    
    public event Action OnInteractComplete;

    private void Awake()
    {
        if(ItemData)
            ItemSprite = ItemData.ItemSprite;
    }
    
    // 아이템 데이터 설정
    public void SetItemData(AvoidItemData data)
    {
        ItemData = data;
        ItemSprite = ItemData.ItemSprite;
        gameObject.name = $"Item_{ItemData.ItemName}";
    }
    
    public string[] GetDialogueTexts()
    {
        if (ItemData != null && ItemData.DialogueTexts is { Length: > 0 })
            return ItemData.DialogueTexts;
        
        // 기본 대화 텍스트
        string itemName = ItemData != null ? ItemData.ItemName : "아이템";
        return new string[] 
        { 
            $"{itemName}을(를) 획득했다."
        };
    }

    
    public void OnInteractionComplete()
    {
        _itemDataEventChannel?.Invoke(ItemData);
        OnInteractComplete?.Invoke();
        
        DeleteItem();
    }
    
    public void ProcessInteraction(AMPlayer player, Action onInteractComplete = null)
    {
        OnInteractComplete += onInteractComplete;
        
        player.TalkBox.StartDialogue
        (
            GetDialogueTexts(), 
            () => 
            {
                Time.timeScale = 1f;
                OnInteractionComplete();
                //OnSpawnMonster?.Invoke(CurrentPos);
            },
            ItemData ? ItemData.ItemSprite : null
        );
    }


    // 아이템 비활성화
    private void DeleteItem()
    {
        IsInteractable = false;
        gameObject.SetActive(false);
    }
}