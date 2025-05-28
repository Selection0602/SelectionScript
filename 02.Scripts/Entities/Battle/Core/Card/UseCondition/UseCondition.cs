using System.Collections.Generic;

public class AllCardUseCondition : ICardUseCondition
{
    public readonly List<ICardUseCondition> _conditions;

    public AllCardUseCondition(List<ICardUseCondition> conditions)
    {
        _conditions = conditions;
    }

    public bool CanUse(Card card, CharacterBase chara, int remainCost)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.CanUse(card, chara, remainCost)) return false;
        }
        return true;
    }
}

/// <summary>
/// 코스트 사용조건
/// </summary>
public class CostCondition : ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara, int remainCost)
    {
        return card.Data.Cost <= remainCost;
    }
}

/// <summary>
/// 소환수가 필요한 카드 사용 조건
/// </summary>
public class SummonRequiredCondition : ICardUseCondition
{
    private HashSet<string> _cardsRequiringSummons;

    public SummonRequiredCondition(HashSet<string> cardsRequiringSummons)
    {
        _cardsRequiringSummons = cardsRequiringSummons;
    }

    public bool CanUse(Card card, CharacterBase chara, int remainCost)
    {
        if (_cardsRequiringSummons.Contains(card.Data.FileName) && chara is ISummoner summoner)
        {
            if (summoner.Summons.Count == 0)
            {
                return false;
            }
        }
        return true;
    }
}

/// <summary>
/// 소환 카드 사용 조건
/// </summary>
public class CanSummonCondition : ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara, int remainCost)
    {
        //소환 카드 & 카드 사용 캐릭터가 소환사 일때
        if (card.Data.FileName == "Calling" && (chara is ISummoner summoner))
        {
            return summoner.IsCanSummon;
        }
        return true;
    }
}

/// <summary>
/// 조종 카드 사용 조건
/// </summary>
public class ManipulationCondition : ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        if (chara is IControllable user)
        {
            if (user.IsControlled && card.Data.FileName != "Manipulation")
            {
                return false;
            }
        }
        return true;
    }
}

/// <summary>
/// 강탈 카드 사용 조건
/// </summary>
public class UseStealCardCondition : ICardUseCondition
{
    private ICardUser _target;

    public UseStealCardCondition(ICardUser target)
    {
        _target = target;
    }

    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        //플레이어의 카드패가 1장이하일때 사용불가
        if (card.Data.FileName == "Extortion" && _target.HandDeck.Count <= 1)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// 디버프 해제 카드 사용 조건
/// </summary>
public class CleanseCondition : ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        //디버프 걸린 상태가 아니라면 디버프 해제 카드 사용 불가
        if (card.Data.FileName == "Panacea" && chara.Debuffs.Count == 0)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// 정신력 회복 카드 사용 조건
/// </summary>
public class HealCondition : ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        //최대 체력인 경우에는 체력 회복 카드 사용불가
        if (card.Data.FileName == "MentalRecovery" && chara.IsFullSanity)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// 등가교환 카드 사용 조건
/// </summary>
public class UseExchangeCondition : ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        //두번 연속으로 등가교환을 쓰고 몬스터가 사망하는 경우가 있기에 일단 데미지 2배
        var damage = card.Data.ApplyValues[0] * 2;
        //데미지를 받고 정신력이 0이하가 되면 사용불가
        if (card.Data.FileName == "Exchange" && chara.WillDieFromDamage(damage))
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// 모독 카드 사용 조건
/// </summary>
public class UseBlasphemyCondition : ICardUseCondition
{
    private ICardUser _target;

    public UseBlasphemyCondition(ICardUser target)
    {
        _target = target;
    }

    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        //플레이어의 카드패가 1장 이하 일때 사용불가
        if (card.Data.FileName == "Blasphemy" && _target.HandDeck.Count <= 1)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// 코스트 회복 카드 사용 조건
/// </summary>
public class UseHealCostCondition : ICardUseCondition
{
    private ICardUser _target;

    public UseHealCostCondition(ICardUser target)
    {
        _target = target;
    }

    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0)
    {
        //코스트가 최대이면 코스트 회복 카드 사용불가
        if (card.Data.FileName == "PotofAvarice" && _target.Cost.IsFullCost())
        {
            return false;
        }
        return true;
    }
}
