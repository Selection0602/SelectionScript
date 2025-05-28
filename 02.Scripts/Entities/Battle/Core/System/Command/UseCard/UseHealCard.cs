using System;
using System.Threading.Tasks;

public class UseHealCard : ICommand
{
    private CharacterBase _user;
    private int _heal;

    public UseHealCard(CharacterBase user, int heal)
    {
        _user = user;
        _heal = heal;
    }

    public async Task Execute()
    {
        _user.Heal(_heal);
        await Task.CompletedTask;
    }

    public void Undo()
    {
        //_user.Damage(_heal);
    }
}
