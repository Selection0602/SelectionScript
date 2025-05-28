using UnityEngine;

/// <summary>
/// 전리품 교환 이벤트 인터페이스
/// </summary>
public interface IBootyExchangeEvent
{
    /// <summary>
    /// 이벤트 적용 후 교환 된 전리품 표시 용
    /// </summary>
    public string Description { get; }
}

public abstract class RandomEventBase : ScriptableObject
{
    [TextArea(3, 5)] public string EventText; // 기본 이벤트 텍스트
    public string YesButtonText; // 예 버튼 텍스트
    public string NoButtonText = "무시한다."; // 아니오 버튼 텍스트
    
    [TextArea(3, 5)] public string YesEventText; // 예 버튼 클릭 시 텍스트
    [TextArea(3, 5)] public string NoEventText; // 아니오 버튼 클릭 시 텍스트
    public Sprite EventSprite; // 이벤트 백그라운드 이미지
    
    public event System.Action OnEventStarted; // 이벤트 시작 시 발생하는 액션
    
    /// <summary>
    /// 이벤트 효과 실행
    /// </summary>
    public abstract void ExecuteEvent();
    
    /// <summary>
    /// 필요 아이템을 가졌는지 확인
    /// </summary>
    /// <returns></returns>
    public abstract bool HasRequiredBooty();
    
    /// <summary>
    /// OnEventStarted 액션 실행
    /// </summary>
    protected virtual void RaiseOnEventStarted() => OnEventStarted?.Invoke();
}
