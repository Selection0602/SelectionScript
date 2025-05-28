using System;
using System.Threading.Tasks;

public class UseChangeCard : ICommand
{
    private ICardUser _user;
    private CardData _changeCardData;

    public UseChangeCard(ICardUser user,CardData data)
    {
        _user = user;
        _changeCardData = data;
    }

    public async Task Execute()
    {
        _user.DeckHandler.ChangeCard(_changeCardData);
        await Task.CompletedTask;
    }

    public void Undo()
    {

    }
}
