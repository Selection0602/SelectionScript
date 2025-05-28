using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TargetOutline : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private Material _material;

    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out Card card))
        {
            _image.material = _material;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.material = null;
    }
}
