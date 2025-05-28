using UnityEngine;

// 특정 전리품을 사용해 공격력 증가 이벤트
[CreateAssetMenu(fileName = "BootyDamageEvent", menuName = "RandomEvents/BootyDamageEvent")]
public class BootyDamageEvent : RandomEventBase, IBootyExchangeEvent
{
    public RewardSO RequiredBooty;
    public int DamageAmount;
    
    public string Description { get; set; }
    
    public override void ExecuteEvent()
    {
        var dataManager = Manager.Instance.DataManager;
        
        dataManager.BounsAttackPower += DamageAmount;
        dataManager.RemoveBooty(RequiredBooty.Index);
        
        Description = $"{RequiredBooty.RewardName}을 건네주고 공격력을 {DamageAmount} 증가시켰습니다.";
    }
    
    public override bool HasRequiredBooty() =>
            Manager.Instance.DataManager.IsExistBooty(RequiredBooty.Index);
}
