using System.Collections.Generic;
using System.Threading.Tasks;

public class EnemyCardCoordinator : ICardDrawer, ICardDistributor
{
    protected BattleManager _battleManager;
    protected EnemyDeckHandler _deckHandler;

    public void SetDeck(DeckHandler deckHandler, int cardIndex)
    {
        _deckHandler = deckHandler as EnemyDeckHandler;
        _deckHandler.Initialize(cardIndex);
    }

    public void DistributeCards(Enemy enemy)
    {
        var cards = GetUseableCards(enemy, GetSortCardList());
        enemy.ReceiveCards(cards);
    }

    private List<Card> GetSortCardList()
    {
        var sortDatas = _deckHandler.HandCards;
        sortDatas.Sort((a, b) => b.Data.CalculatePriority().CompareTo(a.Data.CalculatePriority()));
        return sortDatas;
    }

    private List<Card> GetUseableCards(Enemy enemy, List<Card> cards)
    {
        int remainCost = enemy.Cost.Current;
        List<Card> usableCards = new();

        for (int i = cards.Count - 1; i >= 0; i--)
        {
            var card = cards[i];

            if (enemy.UseCondition.CanUse(card, enemy, remainCost))
            {
                usableCards.Add(card);
                remainCost -= card.Data.Cost;
                cards.RemoveAt(i); // 가져간 카드 제거
            }

            if (remainCost <= 0) break;
        }

        return usableCards;
    }

    public async Task DrawCard(int count, bool isFirstTurn)
    {
       await _deckHandler.DrawCard(count, false);
    }

    public bool IsFullDeck()
    {
        return _deckHandler.IsFullDeck();
    }
}
