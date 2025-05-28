using System;
using System.Threading.Tasks;

public class UseStealCard : ICommand
{
    private CardEffectContext _context;

    public UseStealCard(CardEffectContext context)
    {
        _context = context;
    }

    public async Task Execute()
    {
        var user = _context.SingleTarget as ICardUser;
        var removeCard = user.DeckHandler.GetRemoveCard();
        _context.User.DeckHandler.AddCard(removeCard.Data);
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
