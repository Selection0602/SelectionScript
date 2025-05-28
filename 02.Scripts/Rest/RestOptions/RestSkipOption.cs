using System;

public class RestSkipOption : RestOptionBase
{
    public override string TitleText => "아무 일도 일어나지 않았습니다.";

    private const string REST_PARAM_VALUE = "스킵";

    public override void ApplyOption(Action onComplete)
    {
        Manager.Instance.AnalyticsManager.LogEvent(EventName.REST_SELECTED, EventParam.SELECT_OPTION, REST_PARAM_VALUE);
        onComplete?.Invoke();
    }
}
