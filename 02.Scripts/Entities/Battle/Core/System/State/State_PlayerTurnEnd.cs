using System.Threading.Tasks;

public class State_PlayerTurnEnd : GameStateBase
{
    public State_PlayerTurnEnd(BattleManager manager) : base(manager) { }

    public override async void OnEnter()
    {
        BlockerManager.Instance.Active();
        await ExecuteTurnEndEvents(); //턴 종료 이벤트가 끝날때까지 대기
        StateMachine.MoveNextState(GameState.EnemyTurnStart);
    }

    private async Task ExecuteTurnEndEvents()
    {
        var player = _battleManager.Player;
        await player.OnTurnEnd();
        var enemies = _battleManager.CurrentEnemies;
        foreach (var enemy in enemies)
        {
            await enemy.OnDebuff();
        }
    }
}
