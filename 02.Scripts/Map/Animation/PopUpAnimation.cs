using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopUpAnimation
{
    private const float ANIMATION_DURATION = 0.3f; // 애니메이션 시간

    #region 팝업 열기 및 닫기 애니메이션
    public void PlayOpenAnimation(GameObject targetUI, Button sourceButton, GameObject panel, System.Action onComplete = null)
    {
        panel.SetActive(true);
        targetUI.SetActive(true);
        
        RectTransform uiRect = targetUI.transform as RectTransform;
        if (!uiRect) return;
        
        uiRect.position = sourceButton.transform.position;
        uiRect.localScale = Vector3.zero;
        
        Sequence sequence = DOTween.Sequence();
        // 생성한 시퀀스에 애니메이션 추가
        sequence.Append(uiRect.DOScale(Vector3.one, ANIMATION_DURATION));
        
        // 시퀀스에 포함된 애니메이션 동시에 실행
        sequence.Join(uiRect.DOLocalMove(Vector3.zero, ANIMATION_DURATION).OnComplete(() =>
        {
            onComplete?.Invoke();
        }));
    }
    
    public void PlayCloseAnimation(GameObject targetUI, Button sourceButton, GameObject panel, System.Action onComplete)
    {
        if (!targetUI.activeSelf) return;

        panel.SetActive(false);
        RectTransform uiRect = targetUI.transform as RectTransform;
        if (!uiRect) return;
        
        // 연결된 버튼이 있다면 버튼의 위치로 이동하는 애니메이션 재생
        if (sourceButton != null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(uiRect.DOMove(sourceButton.transform.position, ANIMATION_DURATION));
            sequence.Join(uiRect.DOScale(Vector3.zero, ANIMATION_DURATION));
            sequence.OnComplete(() => onComplete?.Invoke());
        }
        // 연결된 버튼이 없다면 스케일 애니메이션만 재생
        else
        {
            targetUI.transform.DOScale(Vector3.zero, ANIMATION_DURATION)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
    #endregion
}
