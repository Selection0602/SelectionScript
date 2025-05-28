using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GBInteractDetector : MonoBehaviour
{
    public BaseInteractObject CurrentInteractable { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<BaseInteractObject>(out var interactable))
        {
            CurrentInteractable = interactable;
            Debug.Log("Interactable detected: " + interactable.name);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<BaseInteractObject>(out var interactable))
            CurrentInteractable = null;
    }
}
