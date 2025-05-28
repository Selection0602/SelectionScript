using Unity.VisualScripting;
using UnityEngine;

public class State_GameStart : GameStateBase
{
    public State_GameStart(BattleManager manager) : base(manager) { }
    
    public override void OnEnter()
    {
        BlockerManager.Instance.Active();
        _battleManager.OnGameStart?.Invoke();
        GameInitialize();
    }

    private async void GameInitialize()
    {
        await _battleManager.InGameUI.FadeOut();
        var _gameStartInvoker = new CommandInvoker();
        _gameStartInvoker.Enqueue(new DrawCards(_battleManager.Player, 5, true));
        _gameStartInvoker.Enqueue(new DrawCards(_battleManager.EnemyDrawer, 5, true));
        await _gameStartInvoker.ExecuteAllAsync();
        StateMachine.MoveNextState(GameState.PlayerTurnStart);
    }

    public override void OnExit() { }
}
