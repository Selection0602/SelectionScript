using System.Collections.Generic;

public class GameStateMachine : StateMachine<GameState>
{
    private BattleManager _battleManager;

    public GameStateMachine(BattleManager manager) 
    {
        _battleManager = manager;
        StateDic = CreateStateDic();
    }

    protected override Dictionary<GameState, StateBase<GameState>> CreateStateDic()
    {
        return new Dictionary<GameState, StateBase<GameState>>()
                {
                    { GameState.GameStart,  new State_GameStart(_battleManager) },
                    { GameState.PlayerPlay,  new State_PlayerPlay(_battleManager) },
                    { GameState.PlayerTurnStart,  new State_PlayerTurnStart(_battleManager) },
                    { GameState.PlayerTurnEnd,  new State_PlayerTurnEnd(_battleManager) },
                    { GameState.EnemyTurnStart,  new State_EnemyTurnStart(_battleManager) },
                    { GameState.EnemyPlay,  new State_EnemyPlay(_battleManager) },
                    { GameState.EnemyTurnEnd,  new State_EnemyTurnEnd(_battleManager) },
                    { GameState.GameClear,  new State_GameClear(_battleManager) },
                    { GameState.GameOver,  new State_GameOver(_battleManager) },
                };
    }

    public override void MoveNextState(GameState type)
    {
        if (_battleManager.IsGameFinish && type!= GameState.GameClear && type != GameState.GameOver) return;
        base.MoveNextState(type);
    }
}

public class GameStateBase : StateBase<GameState>
{
    protected BattleManager _battleManager;

    public GameStateBase(BattleManager battleManager) : base()
    {
        _battleManager = battleManager;
    }

    public override void OnEnter() { /* 상태 진입 시 처리 */ }
    public override void OnExit() { /* 상태 종료 시 처리 */ }
}
