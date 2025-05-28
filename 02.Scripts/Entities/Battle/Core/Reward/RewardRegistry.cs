using System;
using System.Collections.Generic;

public class RewardEffectRegistry
{
    public static Dictionary<RewardType, Action<RewardData>> RewardEffectMap = new()
    {
        { RewardType.Immediately, GetImmediatelyReward },
        { RewardType.Card, GetCardReward },
        { RewardType.Booty, GetBootyReward },
        { RewardType.Memory, GetMemoryReward }
    };

    private static Dictionary<string, Action<RewardData>> _immediatelyMap = new()
    {
        { "MentalHeal",Heal},
        { "Defcon",IncreaseAttackPower},
        { "Jindotgae",IncreaseAttackPower},
    };

    private static void GetImmediatelyReward(RewardData data)
    {
        _immediatelyMap[data.FileName]?.Invoke(data);
    }

    private static void GetBootyReward(RewardData data)
    {
        if(data.FileName== "Strawberry" || 
            data.FileName == "WhippedCreamCake" || 
            data.FileName == "PoundCake")
        {
            Manager.Instance.DataManager.IncreaseMaxSanity(data.MaxHealValue);
        }
        //전리품 리스트에 추가
        Manager.Instance.DataManager.AddBooty(data);
    }

    private static void GetCardReward(RewardData data)
    {
        //덱에 추가
        Manager.Instance.DataManager.AddCard(data.GetCardIndex);
    }

    private static void GetMemoryReward(RewardData data)
    {
        MemorySO memory;
        
        if(SceneBase.Current is BattleSceneController battleScene)
            memory = battleScene.BattleDataManager.MemoryDatas[data.Index];
        else if (SceneBase.Current is RandomEventScene randomEventScene)
            memory = randomEventScene.EventController.MemoryDict[data.Index];
        else
            throw new Exception("유효하지 않은 씬입니다.");
        
        Manager.Instance.DataManager.AddMemory(memory);
        Heal(data);
        IncreaseAttackPower(data);
    }

    private static void Heal(RewardData data)
    {
        Manager.Instance.DataManager.Heal(data.HealUpValue);
    }

    private static void IncreaseAttackPower(RewardData data)
    {
        Manager.Instance.DataManager.BounsAttackPower += data.AtkUpValue;
    }
}
