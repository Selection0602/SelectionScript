public class State_GameClear : GameStateBase
{
    public State_GameClear(BattleManager manager) : base(manager) { }

    public override async void OnEnter()
    {
        BlockerManager.Instance.InActive();
        _battleManager.InGameUI.HideBattleUI();
        await _battleManager.RewardUI.ShowRewardUI(_battleManager.Node);
        _battleManager.OnGameFinish();
        _battleManager.MoveNextScene(true);
    }
}
