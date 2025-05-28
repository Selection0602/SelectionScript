using System;
using System.Threading.Tasks;

public class UseUpCostCard : ICommand
{
    private ICardUser _user;
    private int _duration;
    private int _increaseCost;

    public UseUpCostCard(ICardUser user, int duration, int cost)
    {
        _user = user;
        _duration = duration;
        _increaseCost = cost;
    }

    public async Task Execute()
    {
        var costUpEffect = new TurnEffect
        {
            Duration = _duration,
            Delay = 1,
            OnTurnStart = () => { _user.Cost.IncreaseMaxCost(_increaseCost); },
            OnTurnEnd = () => { _user.Cost.DecreaseMaxCost(_increaseCost); }
        };

        _user.Character.AddTurnEffect(costUpEffect);

        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
