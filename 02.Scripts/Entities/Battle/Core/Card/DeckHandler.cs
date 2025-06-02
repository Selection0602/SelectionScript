using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class DeckHandler : MonoBehaviour
{
    [SerializeField] private CardPool _cardPool;

    protected Dictionary<string, CardData> _cardDatas = new();
    public Dictionary<string, CardData> CardDatas => _cardDatas;

    [HideInInspector]
    public List<Card> HandCards = new();
    private List<CardData> cardList = new();

    //검색용-------------------------------
    private List<CardData> _handCardDatas = new();
    private HashSet<CardData> _ignoreCardDatas = new();

    protected DataManager _dataManager;

    private int _maxCardCount = 10;

    public event Action<int> OnChangedHandCards = delegate { };

    public void Initialize(int cardIndex)
    {
        _dataManager = Manager.Instance.DataManager;

        SetCanUseCardList(cardIndex);

        foreach (var card in _cardDatas.Values)
        {
            if (card.FileName == "Manipulation")
            {
                IgnoreCard(card);
                continue;
            }
            cardList.Add(card);
        }
    }

    public async UniTask DrawCard(int count, bool isOpen = true, CharacterBase caller = null)
    {
        for (int i = 0; i < count; i++)
        {
            var card = _cardPool.GetCard();
            var data = GetRandomCardData();
            card.Data = data;
            card.SetState(isOpen);
            HandCards.Add(card);
            _handCardDatas.Add(data);
            Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_DrawCard);

            if (caller is Player)
                Manager.Instance.AnalyticsManager.LogEvent
                     (EventName.CARD_DRAWN, EventParam.CARD_NAME, card.Data.CardName);

            await UniTask.Delay(150);
        }
        OnChangedHandCards?.Invoke(HandCards.Count);
    }

    public async UniTask DrawCard(string cardName, bool isOpen = false)
    {
        var card = _cardPool.GetCard();
        var data = _cardDatas[cardName];
        card.Data = data;
        card.SetState(isOpen);
        HandCards.Add(card);
        _handCardDatas.Add(data);
        OnChangedHandCards?.Invoke(HandCards.Count);
        await UniTask.Delay(150);
    }

    private CardData GetRandomCardData()
    {
        int safety = 100;
        while (safety-- > 0)
        {
            var selected = GetRandomWeightedCard(cardList);
            // 핸드에 이미 같은 카드가 있고, ReDraw가 false , 덱에서 파괴 되면 패스
            bool isPass = IsHasHandDeck(selected) || _ignoreCardDatas.Contains(selected);
            if (isPass)
            {
                continue;
            }
            return selected;
        }
        return _cardDatas["Attack"]; //원하는 카드데이터가 안나올시 공격카드 반환
    }

    private CardData GetRandomWeightedCard(List<CardData> cardDatas)
    {
        int totalWeight = 0;

        foreach (var data in cardDatas)
        {
            totalWeight += data.Weight;
        }

        float randomPoint = UnityEngine.Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var data in cardDatas)
        {
            current += data.Weight;
            if (randomPoint <= current)
            {
                return data;
            }
        }
        return cardDatas[0];
    }

    protected abstract void SetCanUseCardList(int index);

    public async Task UseCard(Card card, CardEffectContext context)
    {
        context.CardData = card.Data;

        await card.Use();
        var command = new UseCard(context);
        await command.Execute();
        _cardPool.ReturnCard(card);
        HandCards.Remove(card);
        _handCardDatas.Remove(card.Data);
        OnChangedHandCards?.Invoke(HandCards.Count);
    }

    private bool IsHasHandDeck(CardData card)
    {
        return _handCardDatas.Contains(card) && !card.ReDraw;
    }

    public bool IsFullDeck()
    {
        return HandCards.Count == _maxCardCount;
    }

    public int GetAvailableDrawCount(int amount)
    {
        int space = _maxCardCount - HandCards.Count;
        return Mathf.Min(space, amount);
    }

    public void IgnoreCard(CardData data)
    {
        _ignoreCardDatas.Add(data);
    }

    public void IgnoreCard(string cardName)
    {
        _ignoreCardDatas.Add(_cardDatas[cardName]);
    }

    public void AddCard(CardData data)
    {
        cardList.Add(data);
    }

    public Card GetRemoveCard()
    {
        var randomIndex = RandomUtility.GetRandomIndex(0, HandCards.Count);
        var removeCard = HandCards[randomIndex];
        _cardPool.ReturnCard(removeCard);
        HandCards.Remove(removeCard);
        _handCardDatas.Remove(removeCard.Data);
        OnChangedHandCards?.Invoke(HandCards.Count);
        return removeCard;
    }

    public void ChangeCard(CardData data)
    {
        var randomIndex = RandomUtility.GetRandomIndex(0, HandCards.Count);
        var card = HandCards[randomIndex];
        _handCardDatas.Remove(card.Data);
        _handCardDatas.Add(data);
        OnChangedHandCards?.Invoke(HandCards.Count);
        card.Data = data;
    }

    public bool IsHasManipulationCard()
    {
        var cardData = _dataManager.CardDatas["Manipulation"];
        return _handCardDatas.Contains(cardData);
    }
}
