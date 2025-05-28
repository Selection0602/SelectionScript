using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private const float TEXT_MOVE_Y_DISTANCE = 150f;
    private const float TEXT_MOVE_DURATION = 0.3f;

    private Tweener _currentTween; // 현재 재생 중인 애니메이션 저장
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 텍스트 설정
    /// </summary>
    /// <param name="text">텍스트</param>
    /// <param name="color">색</param>
    public void SetText(string text, Color color)
    {
        _text.color = color;
        _text.text = text;
    }

    /// <summary>
    /// 텍스트 애니메이션 재생
    /// </summary>
    /// <param name="onComplete"></param>
    public async Task PlayTextEffect(Action onComplete = null)
    {
        var completionSource = new TaskCompletionSource<bool>();
        
        _currentTween = transform.DOLocalMove
                (transform.localPosition + new Vector3(0, TEXT_MOVE_Y_DISTANCE, 0), TEXT_MOVE_DURATION).
            OnComplete(() =>
            {
                // 애니메이션 완료 후 TaskCompletionSource를 완료 상태로 설정
                completionSource.TrySetResult(true);
                onComplete?.Invoke();
            })
            .OnKill(() =>
            {
                // 애니메이션이 중단 되었을 때 TaskCompletionSource가 완료되지 않았다면 완료 상태로 설정
                if (!completionSource.Task.IsCompleted)
                    completionSource.TrySetResult(true);
            });
        
        try
        {
            await completionSource.Task;
        }
        catch (TaskCanceledException)
        {
            _currentTween?.Kill();
            throw;
        }
    }

    public void ResetAnimation()
    {
        _currentTween?.Kill();
        _currentTween = null;
    }
}
