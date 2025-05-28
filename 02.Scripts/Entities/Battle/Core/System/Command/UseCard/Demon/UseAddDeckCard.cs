using System;
using System.Threading.Tasks;

public class UseAddDeckCard : ICommand
{
    private ICardUser _user;
    private CardData _addCardData;

    public UseAddDeckCard(ICardUser user,CardData data)
    {
        _user = user;
        _addCardData = data;
    }

    public async Task Execute()
    {
        _user.DeckHandler.AddCard(_addCardData);
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
