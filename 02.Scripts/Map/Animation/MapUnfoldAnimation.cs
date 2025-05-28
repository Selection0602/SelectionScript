using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MapUnfoldAnimation : MonoBehaviour
{
    [Header("Map References")]
    [SerializeField] private ScrollRect _mapScrollRect;

    [Header("Map Borders")] 
    [SerializeField] private GameObject _borderParent;

    [SerializeField] private Transform _leftBorderTransform;
    [SerializeField] private Transform _rightBorderTransform;

    [Header("Animation Positions")] 
    [SerializeField] private Vector2 _leftBorderInitPosition;
    [SerializeField] private Vector2 _rightBorderInitPosition;
    [SerializeField] private Vector2 _leftBorderFinalPosition;
    [SerializeField] private Vector2 _rightBorderFinalPosition;

    [Header("Animation Settings")]
    [SerializeField] private float _scaleDuration = 0.5f;
    [SerializeField] private float _moveDuration = 0.7f;
    [SerializeField] private Ease _moveEase = Ease.OutBack;

    private bool IsAnimationCompleted { get; set; } = false;
    public event Action OnAnimationCompleted;
    
    private void Awake()
    {
        HideMap();
    }

    #region 맵 애니메이션 관련
    /// <summary>
    /// 맵 오픈 애니메이션 재생
    /// </summary>
    public void PlayAnimation()
    {
        IsAnimationCompleted = false;
        ShowMapInitialState();

        StartCoroutine(ClampScrollRect());
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_mapScrollRect.transform.DOScale(Vector3.one, _scaleDuration).SetDelay(0.5f)
            .SetEase(_moveEase));
        sequence.Join(_leftBorderTransform.DOLocalMove(_leftBorderFinalPosition, _moveDuration)
            .SetEase(_moveEase));
        sequence.Join(_rightBorderTransform.DOLocalMove(_rightBorderFinalPosition, _moveDuration)
            .SetEase(_moveEase).OnComplete(() =>
            {
                IsAnimationCompleted = true;
                OnAnimationCompleted?.Invoke();
            }));
    }

    /// <summary>
    /// 맵 오픈 애니메이션 스킵
    /// </summary>
    public void SkipAnimation()
    {
        // 애니메이션 없이 최종 위치로 포지션 수정
        _mapScrollRect.gameObject.SetActive(true);
        _borderParent.SetActive(true);
        
        IsAnimationCompleted = false;

        _mapScrollRect.transform.localScale = Vector3.one;
        _leftBorderTransform.localPosition = _leftBorderFinalPosition;
        _rightBorderTransform.localPosition = _rightBorderFinalPosition;

        _mapScrollRect.horizontalNormalizedPosition = 0f;

        IsAnimationCompleted = true;
        OnAnimationCompleted?.Invoke();
    }
    #endregion

    #region 맵 숨기기 및 초기화
    private void HideMap()
    {
        _mapScrollRect.gameObject.SetActive(false);
        _borderParent.SetActive(false);
    }

    private void ShowMapInitialState()
    {
        _mapScrollRect.gameObject.SetActive(true);
        _borderParent.SetActive(true);

        _mapScrollRect.transform.localScale = new Vector3(0, 1, 1);
        _leftBorderTransform.localPosition = _leftBorderInitPosition;
        _rightBorderTransform.localPosition = _rightBorderInitPosition;
    }

    // 애니메이션 실행 중 스크롤 움직이지 못하도록 고정
    private IEnumerator ClampScrollRect()
    {
        while (!IsAnimationCompleted)
        {
            _mapScrollRect.horizontalNormalizedPosition = 0f;

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
}