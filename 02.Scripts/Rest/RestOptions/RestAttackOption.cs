using System;
using UnityEngine;

public class RestAttackOption : RestOptionBase
{
    public override string TitleText => "공격력이 1 증가했습니다.";
    [SerializeField] private int _bonusAttackPowerValue = 1;

    private const string REST_PARAM_VALUE = "강화";
    
    public override void ApplyOption(Action onComplete)
    {
        Manager.Instance.DataManager.BounsAttackPower += _bonusAttackPowerValue;
        Manager.Instance.AnalyticsManager.LogEvent(EventName.REST_SELECTED, EventParam.SELECT_OPTION, REST_PARAM_VALUE);
        onComplete?.Invoke();
    }
}
