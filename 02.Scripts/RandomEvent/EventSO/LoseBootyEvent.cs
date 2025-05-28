using UnityEngine;

// 랜덤 전리품 제거
[CreateAssetMenu(fileName = "LoseBootyEvent", menuName = "RandomEvents/LoseBootyEvent")]
public class LoseBootyEvent : RandomEventBase
{
    public override void ExecuteEvent()
    {
        var booty = Manager.Instance.DataManager.Booties[Random.Range(0, Manager.Instance.DataManager.Booties.Count)];
        Manager.Instance.DataManager.RemoveBooty(booty);
        
        if(booty.MaxHealValue > 0)
            Manager.Instance.DataManager.DecreaseMaxSanity(booty.MaxHealValue);
    }

    public override bool HasRequiredBooty() => Manager.Instance.DataManager.Booties.Count != 0;
}
