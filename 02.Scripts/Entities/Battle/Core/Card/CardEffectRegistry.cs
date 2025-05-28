using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardEffectRegistry
{
    private static readonly Dictionary<string, Func<CardEffectContext, ICommand>> _cardEffectMap = new();

    public static void Register(string key, Func<CardEffectContext, ICommand> effect)
    {
        _cardEffectMap[key] = effect;
    }

    public static ICommand Create(CardEffectContext context)
    {
        if (_cardEffectMap.TryGetValue(context.CardData.FileName, out var effect))
        {
            return effect(context);
        }
        throw new Exception($"등록되지 않은 카드 효과: {context.CardData.FileName}");
    }
}

public static class CardEffectRegistrar
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void RegisterAll()
    {
        RegisterCommonCards(); // 일반 등급 카드 효과 등록
        RegisterRareCards();
        RegisterEpicCards();
        RegisterLegendaryCards();
        RegisterOphanimCards();
        RegisterDemonCards();
    }

    #region 일반카드
    private static void RegisterCommonCards()
    {
        CardEffectRegistry.Register("Attack", Effect_Attack); //일반 공격
        CardEffectRegistry.Register("DrainLife", Effect_Drain); //흡혈
        CardEffectRegistry.Register("Exchange", Effect_Exchange); //등가 교환
        CardEffectRegistry.Register("RangeAttack", Effect_Attack); //범위 공격
        CardEffectRegistry.Register("ContinuousAttack", Effect_ContinuousAttack); //연속 공격
        CardEffectRegistry.Register("MentalRecovery", Effect_MentalRecovery); //정신력 회복
    }
    private static ICommand Effect_Attack(CardEffectContext effect)
    {
        effect.Damage = effect.CardData.ApplyValues[0];
        return new UseAttackCard(effect.User, effect.Targets,CalculateDamage(effect));
    }
    private static ICommand Effect_Drain(CardEffectContext effect)
    {
        effect.Damage = effect.CardData.ApplyValues[0];

        var invoker = new CommandInvoker();
        invoker.Enqueue(new UseAttackCard(effect.User, effect.Targets, CalculateDamage(effect)));
        invoker.Enqueue(new UseHealCard(effect.User.Character, effect.CardData.ApplyValues[1]));
        return new EnqueueCommand(invoker);
    }
    private static ICommand Effect_Exchange(CardEffectContext effect)
    {
        effect.Damage = effect.CardData.ApplyValues[0];

        var buffCharacter = effect.User.Character;
        if (buffCharacter is ISummoner summoner)
        {
            buffCharacter = summoner.GetlowAttackPowerSummon();
        }

        var invoker = new CommandInvoker();
        invoker.Enqueue(new UseAttackCard(effect.User, new List<CharacterBase> { buffCharacter }, effect.Damage, false));
        invoker.Enqueue(new UseBuffCard(BuffType.Power, buffCharacter, effect.CardData.ApplyValues[1]));
        return new EnqueueCommand(invoker);
    }
    private static ICommand Effect_ContinuousAttack(CardEffectContext effect)
    {
        effect.Damage = effect.CardData.ApplyValues[0];
        effect.ExecuteCount = effect.CardData.ApplyValues[1];

        var invoker = new CommandInvoker();
        for (int i = 0; i < effect.CardData.ApplyValues[1]; i++)
        {
            invoker.Enqueue(new UseAttackCard(effect.User, effect.Targets, CalculateDamage(effect)));
        }
        return new EnqueueCommand(invoker);
    }
    private static ICommand Effect_MentalRecovery(CardEffectContext effect)
    {
        return new UseHealCard(effect.User.Character, effect.CardData.ApplyValues[0]);
    }
    #endregion 일반카드

    #region 레어카드
    private static void RegisterRareCards()
    {
        CardEffectRegistry.Register("ReinforceAttack", Effect_ReinforceAttack); //일반 공격 강화
        CardEffectRegistry.Register("ReinforceDrainLife", Effect_ReinforceDrainLife); //흡혈 강화
        CardEffectRegistry.Register("ReinforceCAttack", Effect_ReinforceCAttack); //연속 공격 강화
        CardEffectRegistry.Register("Panacea", Effect_Panacea); //만병 통치약
        CardEffectRegistry.Register("WillOWisp", Effect_Debuff); //도깨비 불
    }
    private static ICommand Effect_ReinforceAttack(CardEffectContext effect)
    {
        int addAttackPower = effect.CardData.ApplyValues[0];
        return new UseEnhanceCard(EnhanceType.CardDamage, effect.User, new int []{ addAttackPower });
    }
    private static ICommand Effect_ReinforceDrainLife(CardEffectContext effect)
    {
        int addAttackPower = effect.CardData.ApplyValues[0];
        int addHealAmount = effect.CardData.ApplyValues[1];
        return new UseEnhanceCard(EnhanceType.DrainLife, effect.User, new int[] { addAttackPower, addHealAmount });
    }
    private static ICommand Effect_ReinforceCAttack(CardEffectContext effect)
    {
        int addAttackCount = effect.CardData.ApplyValues[0];
        return new UseEnhanceCard(EnhanceType.AttackCount, effect.User, new int[] {addAttackCount });
    }
    private static ICommand Effect_Panacea(CardEffectContext effect)
    {
        return new UseCleanseCard(effect.User);
    }
    private static ICommand Effect_Debuff(CardEffectContext effect)
    {
        var debuffType = (DebuffType)effect.CardData.ApplyValues[0];
        var debuffData = (SceneBase.Current as BattleSceneController).BattleDataManager.DebuffDatas[debuffType];

        return new UseDebuffCard(effect.Targets, debuffData);
    }
    #endregion

    #region 에픽카드
    private static void RegisterEpicCards()
    {
        CardEffectRegistry.Register("Raid", Effect_Raid);
        CardEffectRegistry.Register("PotofGreed", Effect_PotofGreed);
        CardEffectRegistry.Register("PotofGenerosity", Effect_PotofGenerosity);
        CardEffectRegistry.Register("PotofAvarice", Effect_PotofAvarice);
        CardEffectRegistry.Register("PotofDespair", Effect_Debuff);
        CardEffectRegistry.Register("PotofBenevolence", Effect_Debuff);
    }
    private static ICommand Effect_Raid(CardEffectContext effect)
    {
        effect.Damage = effect.CardData.ApplyValues[0];

        var buffCharacter = effect.User.Character;
        if (buffCharacter is ISummoner summoner)
        {
            buffCharacter = summoner.GetlowAttackPowerSummon();
        }

        return new UseTargetKillAtkBuffCard(
            effect.SingleTarget,
            buffCharacter,
            CalculateDamage(effect),
            effect.CardData.ApplyValues[1]
            );
    }
    private static ICommand Effect_PotofGreed(CardEffectContext effect)
    {
        var drawCardCount = effect.CardData.ApplyValues[0];
        return new DrawCards(effect.Drawer, drawCardCount);
    }
    private static ICommand Effect_PotofGenerosity(CardEffectContext effect)
    {
        return new UseUpCostCard(effect.User, 1, effect.CardData.ApplyValues[0]);
    }
    private static ICommand Effect_PotofAvarice(CardEffectContext effect)
    {
        return new UseHealCostCard(effect.User, effect.CardData.ApplyValues[0]);
    }
    #endregion

    #region 오파님 카드
    private static void RegisterOphanimCards()
    {
        CardEffectRegistry.Register("Truth", Effect_Truth);
        CardEffectRegistry.Register("Prophecy", Effect_Prophecy);
        CardEffectRegistry.Register("Calling", Effect_Calling);
        CardEffectRegistry.Register("Command", Effect_Command);
        CardEffectRegistry.Register("Prayer", Effect_Prayer);
        CardEffectRegistry.Register("Blessing", Effect_Blessing);
    }
    private static ICommand Effect_Truth(CardEffectContext effect)
    {
        return new UseTruthCard();
    }
    private static ICommand Effect_Prophecy(CardEffectContext effect)
    {
        return new UseProphecyCard(effect);
    }
    private static ICommand Effect_Calling(CardEffectContext effect)
    {
        var summoner = effect.User as ISummoner;
        return new UseSummonCard(summoner);
    }
    private static ICommand Effect_Command(CardEffectContext effect)
    {
        var summoner = effect.User as ISummoner;
        effect.Damage = summoner.GetSummonBestAttackPower();
        return new UseAttackCard(effect.User, effect.Targets, effect.Damage);
    }
    private static ICommand Effect_Prayer(CardEffectContext effect)
    {
        var invoker = new CommandInvoker();
        var increaseAttackPower = effect.CardData.ApplyValues[0];

        var summoner = effect.User as ISummoner;
        foreach (var summon in summoner.Summons)
        {
            invoker.Enqueue(new UseBuffCard(BuffType.Power, summon, increaseAttackPower));
        }
        return new EnqueueCommand(invoker);
    }
    private static ICommand Effect_Blessing(CardEffectContext effect)
    {
        var summoner = effect.User as ISummoner;
        var needHealSummon = summoner.GetlowSanitySummon();

        return new UseHealCard(needHealSummon, effect.CardData.ApplyValues[0]);
    }
    #endregion

    #region 레전드 카드
    private static void RegisterLegendaryCards()
    {
        CardEffectRegistry.Register("Meditation", Effect_Meditation);
    }
    private static ICommand Effect_Meditation(CardEffectContext effect)
    {
        return new UseBuffCard(BuffType.Cost, effect.User.Character, effect.CardData.ApplyValues[0]);
    }
    #endregion

    #region 데몬 카드
    private static void RegisterDemonCards()
    {
        CardEffectRegistry.Register("Brainwashing", Effect_Brainwashing);
        CardEffectRegistry.Register("Manipulation", Effect_Manipulation);
        CardEffectRegistry.Register("Blasphemy", Effect_Blasphemy);
        CardEffectRegistry.Register("Extortion", Effect_Extortion);
        CardEffectRegistry.Register("Bite", Effect_Attack);
        CardEffectRegistry.Register("Licking", Effect_Drain);
    }
    private static ICommand Effect_Brainwashing(CardEffectContext effect)
    {
        var user = effect.Targets[0] as ICardUser;
        var cardData = Manager.Instance.DataManager.CardDatas["Manipulation"];
        return new UseAddDeckCard(user, cardData);
    }
    private static ICommand Effect_Manipulation(CardEffectContext effect)
    {
        return new UseManipulationCard();
    }
    private static ICommand Effect_Blasphemy(CardEffectContext effect)
    {
        var user = effect.SingleTarget as ICardUser;
        var cardData = Manager.Instance.DataManager.CardDatas["Manipulation"];
        user.DeckHandler.ChangeCard(cardData);
        return new UseChangeCard(user, cardData);
    }
    private static ICommand Effect_Extortion(CardEffectContext effect)
    {
        return new UseStealCard(effect);
    }
    #endregion

    private static int CalculateDamage(CardEffectContext effect)
    {
        return effect.User.Character.AttackLevel + effect.Damage;
    }
}
