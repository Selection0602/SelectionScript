public class State_EnemyTurnEnd : GameStateBase
{
    public State_EnemyTurnEnd(BattleManager manager) : base(manager) { }

    public override async void OnEnter()
    {
        await _battleManager.EnemyTurnEnd();
        StateMachine.MoveNextState(GameState.PlayerTurnStart);
    }
}
