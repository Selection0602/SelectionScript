using System;
using System.Collections.Generic;
using UnityEngine;

public class BootyEffectContext
{
    public RewardData RewardData;
    public Player Player;
    public BattleManager BattleManager;
}

public class BootyEffectRegistry
{
    private static readonly Dictionary<string, Action<BootyEffectContext>> _bootyEffectMap = new();

    public static void Register(string key, Action<BootyEffectContext> effect)
    {
        _bootyEffectMap[key] = effect;
    }

    public static void ApplyEffect(BootyEffectContext context)
    {
        if (_bootyEffectMap.TryGetValue(context.RewardData.FileName, out var effect))
        {
            effect.Invoke(context);
            return;
        }
    }
}

public static class BootyEffectRegistrar
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void RegisterAll()
    {
        BootyEffectRegistry.Register("SpellLump", Effect_SpellLump);
        BootyEffectRegistry.Register("Diary", Effect_Diary);
        BootyEffectRegistry.Register("Trophy", Effect_Trophy);
        BootyEffectRegistry.Register("Earring", Effect_Earring);
        BootyEffectRegistry.Register("CoolingSpray", Effect_CoolingSpray);
        BootyEffectRegistry.Register("SelfDiagnosiskit", Effect_SelfDiagnosiskit);
        BootyEffectRegistry.Register("LoveLetter", Effect_LoveLetter);
        BootyEffectRegistry.Register("LumpOfDesireReward", Effect_LumpOfDesireReward);
    }

    private static void Effect_SpellLump(BootyEffectContext context)
    {
        context.BattleManager.OnGameStart += () =>
        {
            context.Player.Heal(context.RewardData.HealUpValue);
        };
    }

    private static void Effect_Diary(BootyEffectContext context)
    {
        context.Player.OnKillEnemy = async () =>
        {
            await context.Player.DrawCard(context.RewardData.DrawCard, false);
        };
    }

    private static void Effect_Trophy(BootyEffectContext context)
    {
        var enhanceCards = (context.Player.DeckHandler as PlayerDeckHandler).EnhanceCards;

        foreach (var card in enhanceCards)
        {
            card.Cost -= context.RewardData.CostDown;
        }
    }

    private static void Effect_Earring(BootyEffectContext context)
    {
        //전투가 끝나면 정신력 5 회복
        context.BattleManager.OnGameFinish += () =>
        {
            Manager.Instance.DataManager.Heal(context.RewardData.HealUpValue);
        };
    }

    private static void Effect_CoolingSpray(BootyEffectContext context)
    {
        // 화상 디버프 저항
        context.Player.DebuffResistance.Add(DebuffType.Burn);
    }

    private static void Effect_SelfDiagnosiskit(BootyEffectContext context)
    {
        // 독 디버프 저항
        context.Player.DebuffResistance.Add(DebuffType.Poison);
    }

    private static void Effect_LoveLetter(BootyEffectContext context)
    {
        //약화 디버프 저항
        context.Player.DebuffResistance.Add(DebuffType.Weaken);
    }

    private static void Effect_LumpOfDesireReward(BootyEffectContext context)
    {
        //카드 드로우수 증가
        context.BattleManager.PlayerDrawCount += context.RewardData.CardDrawIndex;
    }
}
