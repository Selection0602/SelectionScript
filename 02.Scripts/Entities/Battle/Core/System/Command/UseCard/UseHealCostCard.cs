using System.Threading.Tasks;

public class UseHealCostCard : ICommand
{
    private ICardUser _user;
    private int _healCost;

    public UseHealCostCard(ICardUser user,int healCost)
    {
        _user = user;
        _healCost = healCost;
    }

    public async Task Execute()
    {
        _user.Cost.Add(_healCost);
        await Task.CompletedTask;
    }

    public void Undo()
    {
        _user.Cost.Use(_healCost);
    }
}
