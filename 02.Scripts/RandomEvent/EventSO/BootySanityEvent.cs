using System.Collections.Generic;
using UnityEngine;

// 특정 전리품을 사용해 정신력 회복 이벤트
[CreateAssetMenu(fileName = "BootySanityEvent", menuName = "RandomEvents/BootySanityEvent")]
public class BootySanityEvent : RandomEventBase, IBootyExchangeEvent
{
    public SerializableDic<RewardSO, int> RequiredBootyDict = new SerializableDic<RewardSO, int>();
    private List<RewardSO> _hasRequiredBootyList = new List<RewardSO>();
    
    public string Description { get; private set; }
    
    public override void ExecuteEvent()
    {
        var dataManager = Manager.Instance.DataManager;
        
        // 필요 전리품 리스트에 있는 전리품 중 랜덤으로 선택(플레이어가 가지고 있는 경우만 포함)
        RewardSO randomBooty = _hasRequiredBootyList[Random.Range(0, _hasRequiredBootyList.Count)];
        
        int sanityAmount = RequiredBootyDict[randomBooty];
        
        // 정신력 회복 및 전리품 제거
        dataManager.Heal(sanityAmount);
        
        dataManager.RemoveBooty(randomBooty.Index);
        dataManager.DecreaseMaxSanity(randomBooty.MaxHealValue);
        
        Description = $"{randomBooty.RewardName}을 건네주고 정신력을 {sanityAmount} 회복했습니다.";
    }
    
    public override bool HasRequiredBooty()
    {
        UpdateHasRequiredBooty();
        return _hasRequiredBootyList.Count > 0;
    }
    
    private void UpdateHasRequiredBooty()
    {
        _hasRequiredBootyList.Clear();
        
        foreach (var data in RequiredBootyDict.dataList)
        {
            // RequiredBootyDict에 포함된 전리품을 가지고 있는지 확인 및 리스트에 추가
            if (Manager.Instance.DataManager.IsExistBooty(data.Key.Index))
                _hasRequiredBootyList.Add(data.Key);
        }
    }
}
