using System;
using System.Threading.Tasks;

public class UseProphecyCard : ICommand
{
    private IProphet _prophet;
    private ICardUser _target;
    private CardEffectContext _context;

    public UseProphecyCard(CardEffectContext ctx)
    {
        _context = ctx;
        _prophet = ctx.User as IProphet;
        _target = ctx.SingleTarget as ICardUser;
    }

    public async Task Execute()
    {
        var turnEffect = new TurnEffect
        {
            Duration = 1,
            OnTurnStart = () => { _prophet.Prophecy(_target); }, //타겟이 턴 시작할때 예언 시작
            OnTurnEnd = () => {
                _context.User.DeckHandler.IgnoreCard(_context.CardData); //턴 끝나면 덱에서 예언 카드 제외
            },
        };

        _context.SingleTarget.AddTurnEffect(turnEffect);
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
