using System;
using UnityEngine;

public class Piano : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite _normalImage;
    [SerializeField] private int _requiredItemId;
    [SerializeField] private SafeBox _safeBox;
    
    [SerializeField] private SerializableDic<string, Sprite> _passwordSpriteDict;
    private Sprite _passwordImage;
    
    [SerializeField] private SpriteEventChannel _spriteEventChannel;
    [SerializeField] private FadeOutEventChannel _fadeOutEventChannel;
    
    private bool _hasItem = false;
    private bool _isFirstInteraction = true; // 아이템을 보유한 상태로 처음 상호작용했는지 확인용
    public bool ShouldSpawnMonster { get; private set; } = false;


    public event Action OnInteractComplete;
    public bool IsInteractable { get; private set; } = true;
    public Direction InteractableDirection => Direction.Up;

    private void Awake()
    {
        if (!_safeBox) return;
        
        _passwordImage = _passwordSpriteDict[_safeBox.CorrectPassword];
    }

    public void ProcessInteraction(AMPlayer player, Action onInteractComplete = null)
    {
        OnInteractComplete += onInteractComplete;
        
        if(!_hasItem)
            _hasItem = player.Inventory.HasItem(_requiredItemId);
        
        if(_hasItem && _isFirstInteraction)
        {
            _isFirstInteraction = false;
            ShouldSpawnMonster = true;
            _fadeOutEventChannel?.Invoke(_normalImage, _passwordImage);
            IsInteractable = false;
            return;
        }
        
        _spriteEventChannel?.Invoke(_hasItem ? _passwordImage : _normalImage);
        IsInteractable = false;
    }

    public void OnInteractionComplete()
    {
        if (ShouldSpawnMonster)
        {
            OnInteractComplete?.Invoke();
            ShouldSpawnMonster = false;
        }
        
        _spriteEventChannel?.Invoke(null);
        IsInteractable = true;

    }
}
