using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public Action<PlayerCard, GameObject> OnDropCardEvent = delegate { };

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.TryGetComponent(out PlayerCard card))
        {
            GameObject dropped = eventData.pointerCurrentRaycast.gameObject;
            if (dropped == this.gameObject)
            {
                OnDropCardEvent?.Invoke(card, dropped);
            }
        }
    }
}
