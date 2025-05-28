using System;
using System.Threading.Tasks;

public class UseDrawCard : ICommand
{
    private ICardDrawer _drawer;
    private int _drawCount;

    public UseDrawCard(ICardDrawer drawer, int drawCount)
    {
        _drawer = drawer;
        _drawCount = drawCount;
    }

    public async Task Execute()
    {
        await _drawer.DrawCard(_drawCount, false);
    }

    public void Undo()
    {

    }
}
