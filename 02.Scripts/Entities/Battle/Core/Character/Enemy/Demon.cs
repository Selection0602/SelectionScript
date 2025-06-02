using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Demon : Enemy, IUniqueSkillUser,ICardDrawer
{
    private SkillHandler _skillHandler;
    public SkillSO SkillData => _skillData;
    public bool IsCanUseSkill { get; set; }=true;

    private SkillSO _skillData;

    public override ICardUseCondition UseCondition => CombineConditions(_demonUseConditions);

    private List<ICardUseCondition> _demonUseConditions => new List<ICardUseCondition>()
    {
            new UseStealCardCondition(_manager.Player),
            new UseBlasphemyCondition(_manager.Player),
     };

    public override void Initialize(BattleCharacterData data)
    {
        base.Initialize(data);

        _skillHandler = GetComponent<SkillHandler>();
        _skillData = data.SkillData;
        _skillHandler.Initialize(this);
    }

    public async Task DrawCard(int count, bool isFirstTurn)
    {
        var drawCount = count;

        if (isFirstTurn)
        {
            drawCount -= 1;
           await _deckHandler.DrawCard("Brainwashing");
        }

        await _deckHandler.DrawCard(drawCount, false);
    }

    public bool IsFullDeck()
    {
        return _deckHandler.IsFullDeck();
    }

    public int GetAvailableDrawCount(int amount)
    {
        return _deckHandler.GetAvailableDrawCount(amount);
    }

    public async Task UseSkill()
    {
        if (!IsCanUseSkill) return;
        if (!Cost.IsCanUse(_skillData.PlayCost)) return;
        if (_deckHandler.HandCards.Count > 3) return;
        await _skillHandler.ShowSkillNameUI(_skillData.SkillName);
        IsCanUseSkill = false;
        Cost.Use(_skillData.PlayCost);
        await _deckHandler.DrawCard(_skillData.ValueIndex, false);
        Debug.Log($"{_skillData.SkillName}을 사용했다");
    }
}
