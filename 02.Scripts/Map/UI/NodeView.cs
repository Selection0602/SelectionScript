using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Node _nodeData;
    private Image _nodeImage;
    private Tween _currentAnimation;
    private Vector3 _originalScale;

    public Outline Outline { get; private set; }

    public RectTransform RectTransform { get; private set; }

    [SerializeField] private NodeEventChannel _nodeClickEventChannel;

    [Header("Animation Settings")]
    [SerializeField] private float _doNodeScale = 1.2f; // 노드 확대 비율
    [SerializeField] private float _doMoveableNodeDuration = 1f; // 이동 가능 노드 애니메이션 시간
    [SerializeField] private float _doEnterNodeDuration = 0.2f; // 커서 노드 확대 애니메이션 시간

    private void Awake()
    {
        RectTransform = transform as RectTransform;
    }

    #region 노드 뷰 초기화
    /// <summary>
    /// 노드 뷰 초기화
    /// </summary>
    /// <param name="nodeData">노드 뷰에서 가지고 있을 노드의 정보</param>
    /// <param name="nodeTypeData">노드 타입 정보</param>
    public void Setup(Node nodeData, NodeTypeData nodeTypeData)
    {
        _nodeImage = GetComponent<Image>();
        Outline = GetComponent<Outline>();

        Outline.enabled = false;
        
        _nodeImage.sprite = nodeTypeData.NodeIcon;
        _nodeImage.color = nodeTypeData.NodeColor;
        
        _nodeImage.transform.localScale = nodeTypeData.NodeScale;
        _originalScale = nodeTypeData.NodeScale;
        
        _nodeData = nodeData;
    }
    #endregion

    #region 노드 색상 변경
    /// <summary>
    /// 방문한 노드 색 변경 함수(살짝 어둡게)
    /// </summary>
    public void ChangeColor(float colorOffset)
    {
        Color originalColor = _nodeImage.color;

        // 원래 색보다 어둡게 변경
        Color visitedColor = originalColor * colorOffset;
        visitedColor.a = originalColor.a;
        _nodeImage.color = visitedColor;
    }
    #endregion

    #region 노드 애니메이션
    /// <summary>
    /// 이동 가능 애니메이션 시작
    /// </summary>
    public void StartMoveableAnimation()
    {
        _currentAnimation = transform.DOScale(_originalScale * _doNodeScale, _doMoveableNodeDuration).
            SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    /// <summary>
    /// 이동 가능 애니메이션 정지
    /// </summary>
    public void StopMoveableAnimation()
    {
        // 현재 애니메이션이 있다면 정지
        _currentAnimation?.Kill();
        _currentAnimation = null;
        
        transform.localScale = _originalScale;
    }
    #endregion

    #region 포인터 이벤트
    public void OnPointerClick(PointerEventData eventData)
    {
        // 현재 노드로 플레이어 위치 업데이트 요청
        if (eventData.button == PointerEventData.InputButton.Left)
            _nodeClickEventChannel?.Invoke(_nodeData);
    }

    // 마우스 포인터가 노드 위에 올라갔을 때 확대
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 애니메이션 재생 중이라면 일시정지 후 확대
        if (_currentAnimation != null)
        {
            _currentAnimation.Pause();
            Outline.enabled = true;
        }
        transform.DOScale(_originalScale * _doNodeScale, _doEnterNodeDuration).SetEase(Ease.OutBack);
    }

    // 마우스 포인터가 노드 위에서 벗어났을 때 원래 크기로 축소
    public void OnPointerExit(PointerEventData eventData)
    {
        // 애니메이션이 일시정지 상태라면 재생
        if (_currentAnimation != null)
        {
            _currentAnimation.Play();
            Outline.enabled = false;
        }
        else
            transform.DOScale(_originalScale, _doEnterNodeDuration).SetEase(Ease.OutBack);
    }
    #endregion

}