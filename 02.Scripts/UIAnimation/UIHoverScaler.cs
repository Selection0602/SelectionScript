using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector2 _baseScale = new();

    [SerializeField] private Vector2 _hoverScale = new Vector2(1.1f, 1.1f);
    [SerializeField] private float _duration = 0.2f;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(_hoverScale, _duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_baseScale, _duration);
    }
}
