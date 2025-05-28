using System.Threading.Tasks;

public class Maria : Player
{
    public override void Initialize(BattleCharacterData data)
    {
        base.Initialize(data);

        _skillHandler = GetComponent<SkillHandler>();
        _skillData = data.SkillData;
        _skillHandler.Initialize(this);
    }

    public override async Task UseSkill()
    {
        if (!IsCanUseSkill) return;
        if (!Cost.IsCanUse(_skillData.PlayCost)) return; //코스트가 충분하지않다면 사용 불가
        IsCanUseSkill = false;
        BlockerManager.Instance.Active();
        await _skillHandler.ShowSkillNameUI(_skillData.SkillName);
        Manager.Instance.AnalyticsManager.LogEvent(EventName.SKILL_USED, EventParam.SKILL_NAME, _skillData.SkillName);
        BlockerManager.Instance.InActive();
        Cost.Use(_skillData.PlayCost); //코스트 사용
        Heal(_skillData.ValueIndex); //체력 회복
    }
}
