public class State_EnemyPlay : GameStateBase
{
    public State_EnemyPlay(BattleManager manager) : base(manager) { }

    public override async void OnEnter()
    {
        await _battleManager.EnemyAttack();
        StateMachine.MoveNextState(GameState.EnemyTurnEnd);
    }
}
