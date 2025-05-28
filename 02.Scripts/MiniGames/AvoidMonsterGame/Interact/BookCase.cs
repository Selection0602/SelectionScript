using System;
using DG.Tweening;
using UnityEngine;

public class BookCase : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; private set; } = true;
    
    [SerializeField] private Direction _interactableDirection = Direction.Up;
    public Direction InteractableDirection => _interactableDirection;
    public event Action OnInteractComplete;
    
    private const float MOVE_DISTANCE = 1.02f;
    private const float MOVE_DURATION = 2f;
    
    public void OnInteractionComplete()
    {
        IsInteractable = false;
        OnInteractComplete?.Invoke();
    }

    public void ProcessInteraction(AMPlayer player, Action onInteractComplete = null)
    {
        Time.timeScale = 1f;
        
        player.ChangeInput("Empty");
        
        OnInteractComplete += onInteractComplete;
        OnInteractComplete += () => player.ChangeInput("Player");
        
        float targetX = transform.localPosition.x - MOVE_DISTANCE;
        transform.DOLocalMoveX(targetX, MOVE_DURATION).OnComplete(OnInteractionComplete);
    }
}
