using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _moveY;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector2 _modifyScale;
    [SerializeField] private float _modifySpeed;

    private Canvas _canvas;
    private RectTransform _rectTransform;

    private Vector3 _offset;
    private Vector2 _basePos;
    private Vector2 _baseScale;
    private Quaternion _baseRotate;

    private int _index; //히어라키 순서

    private static bool _isDragging = false;

    public bool IsUseCard = false;

    public Action OnUpdateCardSibling = delegate { };

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _basePos = _rectTransform.localPosition;
        _baseScale = transform.localScale;
        _baseRotate = _rectTransform.localRotation;
    }

    public void SetBasePosition(Vector2 pos)
    {
        _basePos = pos;
        _rectTransform.DOLocalMove(pos, 0.3f).OnComplete(() => 
        {
            if (!_canvasGroup.blocksRaycasts)
            {
                _canvasGroup.blocksRaycasts = true;
            }
        });
    }

    public void SetBaseRotate(Quaternion rotate)
    {
        _baseRotate = rotate;
        _rectTransform.DOLocalRotate(rotate.eulerAngles, 0.3f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _rectTransform.DOKill();
        //드래그 시작 시 마우스와 UI 오브젝트 간의 거리를 저장
        _canvasGroup.blocksRaycasts = false;
        _offset = _rectTransform.localPosition - GetLocalMousePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.localScale = Vector3.one; //확대된 상태를 원래대로
        _rectTransform.localPosition = GetLocalMousePosition(eventData) + _offset;
    }

    public void ActiveBlocksRaycasts()
    {
        _canvasGroup.blocksRaycasts = true;
    }

    private Vector3 GetLocalMousePosition(PointerEventData eventData)
    {
        // 마우스의 스크린 좌표를 부모 기준의 로컬 좌표계로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform, // 기준을 부모 rectTransform로 (UI의 anchoredPosition은 부모 rectTransform 기준이기 때문)
            eventData.position, //마우스의 화면 좌표
            Camera.main, // worldcamera 기준으로 변환
            out Vector2 localPointerPos);
        return localPointerPos;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;
        _index = transform.GetSiblingIndex();
        _rectTransform.DOLocalRotate(Vector3.zero, 0.3f);
        _rectTransform.DOScale(_modifyScale, _modifySpeed);
        _rectTransform.DOLocalMoveY(_moveY, _moveSpeed);
        _rectTransform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDragging) return;
        _rectTransform.DOLocalRotate(_baseRotate.eulerAngles, 0.3f);
        _rectTransform.DOLocalMoveY(_basePos.y, _moveSpeed);
        _rectTransform.DOScale(_baseScale, _modifySpeed);
        OnUpdateCardSibling?.Invoke();
    }

    public void ResetData()
    {
        _isDragging = false;
        _canvasGroup.blocksRaycasts = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsUseCard) return;
        OnEndDrag();
    }

    public void OnEndDrag()
    {
        SetBasePosition(_basePos);
        SetBaseRotate(_baseRotate);
        OnUpdateCardSibling?.Invoke();
        _isDragging = false;
        IsUseCard = false;
    }
}
