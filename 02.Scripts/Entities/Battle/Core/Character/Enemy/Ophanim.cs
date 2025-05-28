using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 오파님 보스 클래스
/// LeaderEnemyBase (카드 분배하는 리더 역할)
/// IProphet (예언자 역할)
/// ISummoner (소환자 역할)
/// </summary>
public class Ophanim : Enemy, IProphet, ISummoner, IUniqueSkillUser, ICardDrawer
{
    private SkillHandler _skillHandler;
    private Material _blinkMat;
    private MeshRenderer meshRenderer;

    public override ICardUseCondition UseCondition => CombineConditions(_ophanimUseConditions);

    private List<ICardUseCondition> _ophanimUseConditions => new List<ICardUseCondition>()
    {
            new SummonRequiredCondition(_cardsRequiringSummons),
            new CanSummonCondition(),
     };

    public override void Initialize(BattleCharacterData data)
    {
        base.Initialize(data);

        _skillHandler = GetComponent<SkillHandler>();
        _skillData = data.SkillData;
        _skillHandler.Initialize(this);

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        _blinkMat = meshRenderer.materials[0];
    }

    public override async Task PlayAttackAnim()
    {
        await Task.CompletedTask;
    }

    public override async Task Dead()
    {
        _animController.StopAnimation();
        _attackPowerText.gameObject.SetActive(false);
        await PlayDeadEffect();
        gameObject.SetActive(false);
        await _manager.InGameUI.FadeInOut_White();
        _manager.OnEnemyDied(this);
    }

    private int _blinkCount = 3;
    private int _blinkDuration = 100;

    private async Task PlayDeadEffect()
    {
        _characterImage.material = _blinkMat;
        for (int i = _blinkCount; i >= 1; i--)
        {
            _characterImage.material.SetFloat("_BlinkAmount", 1);
            await UniTask.Delay(_blinkDuration * i);
            _characterImage.material.SetFloat("_BlinkAmount", 0);
            await UniTask.Delay(_blinkDuration * i);
        }
    }

    public async Task DrawCard(int count, bool isFirstTurn)
    {
        var drawCount = count;

        if (isFirstTurn)
        {
            drawCount -= 2;
            await _deckHandler.DrawCard("Truth"); // 진실 카드를 가지고 시작 
            await _deckHandler.DrawCard("Calling"); // 부름 카드를 가지고 시작 
            _deckHandler.IgnoreCard("Truth"); // 이 다음부터는 진실 카드를 뽑기에서 무시
        }

        await _deckHandler.DrawCard(drawCount, false);
    }

    public bool IsFullDeck()
    {
        return _deckHandler.IsFullDeck();
    }

    public override async Task Damage(int amount)
    {
        if (_summons.Count > 0) return;
        await base.Damage(amount);
    }

    #region 예언자 역할
    public CardData PredictedCard { get; set; }

    public void Prophecy(ICardUser target)
    {
        Card bestCard = null;
        int bestPriority = int.MaxValue;

        for (int i = 0; i < target.HandDeck.Count; i++)
        {
            var card = target.HandDeck[i];
            int priority = card.Data.CalculatePriority();

            if (priority < bestPriority)
            {
                bestPriority = priority;
                bestCard = card;
            }
        }
        PredictedCard = bestCard?.Data;
    }

    public void MissedProphecy()
    {
        throw new System.NotImplementedException();
    }

    public void SucceededProphecy()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region 소환사 역할
    public bool IsCanSummon => _summons.Count < _maxSummonCount;
    public bool IsCanDamaged => _summons.Count == 0;
    public List<Summon> Summons => _summons;

    private int _maxSummonCount = 2;
    private List<Summon> _summons = new();
    private HashSet<string> _cardsRequiringSummons = new() { "Command", "Prayer", "Blessing", "Exchange" };

    public void Summon()
    {
        var summon = _manager.BattleSetup.GetSummon(_summons.Count);
        summon.SetSummoner(this);
        _summons.Add(summon);
    }

    public void DeathSummon(Summon valkyrie)
    {
        _summons.Remove(valkyrie);
    }

    public Summon GetlowSanitySummon()
    {
        var needHealSummon = _summons[0];
        foreach (var summon in _summons)
        {
            //정신력이 낮은 소환수 선정
            if (summon.Sanity < needHealSummon.Sanity)
            {
                needHealSummon = summon;
            }
        }
        return needHealSummon;
    }

    public Summon GetlowAttackPowerSummon()
    {
        if (_summons.Count == 0)
        {
            return null;
        }

        var needBuffSummon = _summons[0];
        foreach (var summon in _summons)
        {
            //공격력이 낮은 소환수 선정
            if (summon.AttackPower > needBuffSummon.AttackPower)
            {
                needBuffSummon = summon;
            }
        }
        return needBuffSummon;
    }

    public int GetSummonBestAttackPower()
    {
        int bestAttackPower = 0;
        foreach (var summon in _summons)
        {
            if (summon.AttackPower > bestAttackPower)
            {
                bestAttackPower = (int)summon.AttackPower;
            }
        }
        return bestAttackPower;
    }
    #endregion

    public SkillSO SkillData => _skillData;
    private SkillSO _skillData;

    public bool IsCanUseSkill { get; set; } = true;

    public async Task UseSkill()
    {
        if (!IsCanUseSkill) return;
        if (!Cost.IsCanUse(_skillData.PlayCost)) return;
        await _skillHandler.ShowSkillNameUI(_skillData.SkillName);
        IsCanUseSkill = false;
        Cost.Use(_skillData.PlayCost);
        await _manager.Player.Damage(_skillData.ValueIndex);
        Debug.Log($"{_skillData.SkillName}을 사용했다");
    }
}
