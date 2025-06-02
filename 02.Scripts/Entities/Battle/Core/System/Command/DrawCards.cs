using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 카드 배포 커맨드
/// </summary>
public class DrawCards : ICommand
{
    private ICardDrawer _drawer;
    private int _amount;
    private bool _isFirstTurn;

    public DrawCards(ICardDrawer drawer, int amount,bool isFirstTurn=false)
    {
        _drawer = drawer;
        _amount = amount;
        _isFirstTurn = isFirstTurn;
    }

    public async Task Execute()
    {
        //현재 가지고 있는 카드패의 수가 최대이면 드로우 불가능
        if (_drawer.IsFullDeck()) return;
        int drawCount = _drawer.GetAvailableDrawCount(_amount);
        await _drawer.DrawCard(drawCount, _isFirstTurn);
    }

    public void Undo()
    {
        // 필요하면 구현
    }
}
