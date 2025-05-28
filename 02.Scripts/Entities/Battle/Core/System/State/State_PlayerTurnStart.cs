using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class State_PlayerTurnStart : GameStateBase
{
    public State_PlayerTurnStart(BattleManager manager) : base(manager) { }

    private CancellationTokenSource _cts = new();

    public override async void OnEnter()
    {
        await StartPlayerTurn();
    }

    private async Task StartPlayerTurn()
    {
        if (_battleManager.IsGameFinish) return;
        try
        {
            await _battleManager.InGameUI.ShowFloatingText("플레이어 턴");
            await _battleManager.PlayerTurnStart();
            StateMachine.MoveNextState(GameState.PlayerPlay);
        }
        catch (Exception e)
        {
            Debug.Log("floatingText의 비동기 처리가 남아있음");
        }      
    }
}
