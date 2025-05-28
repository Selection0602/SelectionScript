public class EnemyDeckHandler : DeckHandler
{
    protected override void SetCanUseCardList(int index)
    {
        foreach (var data in _dataManager.GetDefaultDeck(index).Values)
        {
            _cardDatas.Add(data.FileName, data.Clone());
        }
    }
}



