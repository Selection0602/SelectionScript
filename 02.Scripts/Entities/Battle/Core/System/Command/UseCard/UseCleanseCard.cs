using System;
using System.Threading.Tasks;

public class UseCleanseCard : ICommand
{
    private ICardUser _user;
  
    public UseCleanseCard(ICardUser user)
    {
        _user = user;
    }

    public async Task Execute()
    {
        _user.Character.AllRemoveDebuff();
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
