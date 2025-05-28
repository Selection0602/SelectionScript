using System.Threading.Tasks;

public class State_GameOver : GameStateBase
{
    public State_GameOver(BattleManager manager) : base(manager) { }

    public override void OnEnter()
    {
        BlockerManager.Instance.InActive();
        _battleManager.MoveNextScene(false);
    }
}
