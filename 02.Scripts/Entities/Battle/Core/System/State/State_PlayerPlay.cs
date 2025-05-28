public class State_PlayerPlay : GameStateBase
{
    public State_PlayerPlay(BattleManager manager) : base(manager) { }

    private TutorialController _tutorialController;
    
    public override void OnEnter()
    {
        BlockerManager.Instance.InActive();

        _tutorialController ??= ServiceLocator.GetService<TutorialController>();
        if(_battleManager.Node != NodeType.BossBattle)
            _tutorialController?.StartTutorial();
        else 
            _tutorialController?.StartBossSkillTutorial();
    }
}
