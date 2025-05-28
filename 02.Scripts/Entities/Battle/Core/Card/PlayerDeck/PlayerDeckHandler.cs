using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckHandler : DeckHandler
{
    public List<CardData> EnhanceCards => _enhanceCards;
    private List<CardData> _enhanceCards = new();

    private List<int> _enhanceCardIndexList = new(){ 7,8,9 };

    [SerializeField] private List<string> _testCardNameList;

    protected override void SetCanUseCardList(int index)
    {
#if UNITY_EDITOR
        if (_testCardNameList.Count > 0)
        {
            foreach(var cardName in _testCardNameList)
            {
                _cardDatas.Add(cardName, _dataManager.CardDatas[cardName].Clone());
            }
        }
        else
        {
            foreach(var data in _dataManager.PlayerDeck.Values)
            {
                _cardDatas.Add(data.FileName,data.Clone());
            }
        }
#else
          foreach(var data in _dataManager.PlayerDeck.Values)
          {
              _cardDatas.Add(data.FileName,data.Clone());
           }
#endif

        foreach (var data in _cardDatas.Values)
        {
            if(_enhanceCardIndexList.Contains(data.Index))
            {
                _enhanceCards.Add(data);
            }
        }
    }
}
