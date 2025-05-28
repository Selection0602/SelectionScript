using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.U2D;

public class DataManager
{
    public int CurrentCharcterId = 2;

    public int BounsAttackPower = 0;

    public CharacterSO CharacterData { get; private set; }

    //캐릭터 설정 
    public async Task InitializeCharacter(CharacterSO data)
    {
        CharacterData = data;
        CurrentCharcterId = data.Index; //캐릭터 ID설정
        _sanity = new Sanity(data.Health, data.Health); //캐릭터 정신력 설정

        await LoadCardDatas(); //카드 데이터 로드
        _playerDeck = GetDefaultDeck(data.CardIndex);  //캐릭터 기본 덱 설정

        Manager.Instance.AnalyticsManager.LogEvent(EventName.CHARACTER_SELECTED, EventParam.CHARACTER_NAME, data.Name);
    }

    public void ClearCharacterData()
    {
        CharacterData = null;
        CurrentCharcterId = 2;
        _sanity = new Sanity(0, 0);
        _playerDeck.Clear();
        _booties.Clear();
        _memories.Clear();
        BounsAttackPower = 0;

        _cardDatas = new Dictionary<string, CardData>();
    }

    #region 게임정신력
    public event Action OnSanityChanged;

    public int CurrentSanity => _sanity.CurrentSanity;
    public int MaxSanity => _sanity.MaxSanity;

    private Sanity _sanity;

    public void Heal(int amount)
    {
        _sanity.Heal(amount);
        OnSanityChanged?.Invoke();
    }

    public void IncreaseMaxSanity(int amount)
    {
        _sanity.IncreaseMaxSanity(amount);
        OnSanityChanged?.Invoke();
    }
    
    public void DecreaseMaxSanity(int amount)
    {
        _sanity.DecreaseMaxSanity(amount);
        OnSanityChanged?.Invoke();
    }
    
    public void Damage(int amount)
    {
        _sanity.Damage(amount);
        OnSanityChanged?.Invoke();
    }
    #endregion

    #region 카드 덱
    public IReadOnlyDictionary<string, CardData> CardDatas => _cardDatas; //모든 카드 데이터 참조용
    private Dictionary<string, CardData> _cardDatas = new(); //모든 카드 데이터

    public Dictionary<string, CardData> PlayerDeck => _playerDeck; //플레이어 덱 데이터 참조용
    private Dictionary<string, CardData> _playerDeck = new(); //플레이어 덱 데이터 

    private readonly Dictionary<int, List<CardRarityType>> _cardIndexMap = new()
    {
        { 1, new() { CardRarityType.Common } },
        { 2, new() { CardRarityType.Common, CardRarityType.Rare } },
        { 3, new() { CardRarityType.Common, CardRarityType.Rare, CardRarityType.Epic } },
        { 5, new() { CardRarityType.Common, CardRarityType.Rare, CardRarityType.Epic ,CardRarityType.Ophanim} },
        { 6, new() { CardRarityType.Common, CardRarityType.Rare, CardRarityType.Epic,CardRarityType.Demon } }
    };

    //카드 데이터 로드해서 CardData클래스로 파싱
    public async Task LoadCardDatas()
    {
        var allCards = await Manager.Instance.AddressableManager.GetHandleResultList<CardSO>("Card");
        _cardDatas.Clear();
        foreach (var data in allCards)
        {
            _cardDatas.Add(data.FileName, new CardData().Clone(data));
        }
    }

    //기본 덱 데이터 반환
    public Dictionary<string, CardData> GetDefaultDeck(int cardIndex)
    {
        var grades = _cardIndexMap[cardIndex];
        var cardList = new Dictionary<string, CardData>();

        foreach (var card in _cardDatas)
        {
            if (grades.Contains(card.Value.RarityType))
            {
                cardList.Add(card.Key, card.Value);
            }
        }
        return cardList;
    }

    //덱에 카드 추가
    public void AddCard(int cardIndex)
    {
        var cardData = GetCardData(cardIndex);
        _playerDeck.Add(cardData.FileName, cardData);
    }

    public bool IsCanAddCard(int cardIndex)
    {
        var cardData = GetCardData(cardIndex);
        return !_playerDeck.ContainsKey(cardData.FileName);
    }

    //인덱스로 카드 데이터 반환
    public CardData GetCardData(int index)
    {
        foreach (var data in _cardDatas.Values)
        {
            if (data.Index == index)
            {
                return data;
            }
        }
        return null;
    }
    #endregion

    #region 전리품
    public event Action OnBootyChanged;
    private List<RewardData> _booties = new();
    public IReadOnlyList<RewardData> Booties => _booties;

    public void AddBooty(RewardData data)
    {
        _booties.Add(data);
        OnBootyChanged?.Invoke();
    }

    public void RemoveBooty(RewardData data)
    {
        _booties.Remove(data);
        OnBootyChanged?.Invoke();
    }
    
    public void RemoveBooty(int index)
    {
        var data = _booties.Find(x => x.Index == index);
        if (data == null) return;
    
        _booties.Remove(data);
        OnBootyChanged?.Invoke();
    }
    
    public bool IsExistBooty(RewardData data)
    {
        foreach (var booty in _booties)
        {
            if (booty.Index == data.Index)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsExistBooty(int index)
    {
        foreach(var booty in _booties)
        {
            if(booty.Index == index)
            {
                return true;
            }
        }
        return false;
    }
    
    public void ApplyBootyEffects(BattleManager battleManager, Player player)
    {
        var bootyEffectContext = new BootyEffectContext
        {
            BattleManager = battleManager,
            Player = player
        };
        foreach (var booty in _booties)
        {
            bootyEffectContext.RewardData = booty;
            BootyEffectRegistry.ApplyEffect(bootyEffectContext);
        }
    }
    #endregion

    #region 메모리
    private Dictionary<int, MemorySO> _memories = new();
    public Dictionary<int, MemorySO> Memories => _memories;

    public void AddMemory(MemorySO memory)
    {
        _memories.Add(memory.Index, memory);
    }
    #endregion

    #region 엔딩
    private HashSet<int> _viewedEndings = new HashSet<int>(); // 기본 엔딩도 처음에는 잠김 상태
    public HashSet<int> ViewedEndings => _viewedEndings;

    public void AddViewedEnding(int endingIndex)
    {
        _viewedEndings.Add(endingIndex);
    }

    public bool HasViewedEnding(int endingIndex)
    {
        return _viewedEndings.Contains(endingIndex);
    }
    #endregion
    #region 미니게임
    private HashSet<int> _unlockedMiniGames = new HashSet<int>();
    public HashSet<int> UnlockedMiniGames => _unlockedMiniGames;

    public void AddUnlockedMiniGame(int miniGameIndex)
    {
        _unlockedMiniGames.Add(miniGameIndex);
    }

    public bool HasUnlockedMiniGame(int miniGameIndex)
    {
        return _unlockedMiniGames.Contains(miniGameIndex);
    }
    #endregion
}