using System;
using UnityEngine;

public class RestHealOption : RestOptionBase
{
    public override string TitleText => "정신력이 10 회복 되었습니다.";
    [SerializeField] private int _healValue = 10;
    
    private const string REST_PARAM_VALUE = "회복";
    
    public override void ApplyOption(Action onComplete)
    {
        Manager.Instance.DataManager.Heal(_healValue);
        Manager.Instance.AnalyticsManager.LogEvent(EventName.REST_SELECTED, EventParam.SELECT_OPTION, REST_PARAM_VALUE);
        onComplete?.Invoke();
    }
}
