using System;
using UnityEngine;

public class SafeBox : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite _openSprite;
    [SerializeField] private GameObject _spawnItem;
    [SerializeField] private AvoidItemData _spawnItemData; // 금고 열었을 때 생성할 아이템
    private SpriteRenderer _currentSprite;
    
    [SerializeField] private string[] _passwords;
    private string _correctPassword;
    public string CorrectPassword => _correctPassword;
    
    public bool IsInteractable { get; private set; } = true;
    public Direction InteractableDirection { get; } = Direction.Up;
    public event Action OnInteractComplete;

    private Action _onOpenSafeBox;
    
    private void Awake()
    {
        _currentSprite = GetComponent<SpriteRenderer>();
        _correctPassword = _passwords[UnityEngine.Random.Range(0, _passwords.Length)];
        
        _onOpenSafeBox += OnOpenSafeBoxComplete;
    }

    public void ProcessInteraction(AMPlayer player, Action onInteractComplete = null)
    {
        OnInteractComplete += onInteractComplete;
        player.ChangeInput("UI");
        player.MenuUI.OpenPasswordUI(_correctPassword, _onOpenSafeBox);
    }

    public void OnInteractionComplete()
    {
        OnInteractComplete?.Invoke();
    }

    private void OnOpenSafeBoxComplete()
    {
        _currentSprite.sprite = _openSprite;
        
        GameObject itemObject = Instantiate(_spawnItem,
            transform.position, Quaternion.identity, transform.parent);

        ItemForEscape item = itemObject.GetComponent<ItemForEscape>();
        item?.SetItemData(_spawnItemData);

        IsInteractable = false;
    }
}
