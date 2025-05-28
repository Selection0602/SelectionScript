using System;
using UnityEngine;

public class InteractDetector : MonoBehaviour
{
    public IInteractable CurrentInteractable { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
            CurrentInteractable = interactable;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
            CurrentInteractable = null;
    }
}
