public class State_EnemyTurnStart : GameStateBase
{
    public State_EnemyTurnStart(BattleManager manager) : base(manager) { }

    public override async void OnEnter()
    {
        if (_battleManager.IsGameFinish) return;
        await _battleManager.InGameUI.ShowFloatingText("상대 턴");
        await _battleManager.EnemyTurnStart();
        StateMachine.MoveNextState(GameState.EnemyPlay);
    }
}
