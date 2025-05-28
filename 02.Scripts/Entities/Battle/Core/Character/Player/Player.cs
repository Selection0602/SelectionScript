using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class Player : CharacterBase
    , ICardUser
    , IControllable
    , IUniqueSkillUser
    , ICardDrawer
{
    [SerializeField] private PlayerDeckHandler _deckHandler;

    public CharacterBase Character => this;
    public List<Card> HandDeck => _deckHandler.HandCards;
    public Dictionary<string, CardData> Decks => _decks;
    public Cost Cost { get; set; }
    public DeckHandler DeckHandler => _deckHandler;
    public bool IsControlled => _deckHandler.IsHasManipulationCard();

    private Dictionary<string, CardData> _decks = new();

    //유물용
    public Action OnKillEnemy = delegate { }; //적을 처치했을때 호출되는 이벤트
    public List<DebuffType> DebuffResistance = new(); //디버프 저항 리스트

    public override void Initialize(BattleCharacterData data)
    {
        _characterImage = GetComponent<Image>();
        _characterImage.sprite = data.Sprite;
        _effectPos = _characterImage.transform;

        base.Initialize(data);

        _deckHandler = GetComponent<PlayerDeckHandler>();
        _deckHandler.Initialize(data.CardIndex);

        foreach (var cardData in _deckHandler.CardDatas)
        {
            _decks.Add(cardData.Key, cardData.Value);
        }
        Cost = new Cost(data.Cost);

        //유니크 스킬 셋팅
        _skillData = data.SkillData;
        _skillHandler = GetComponent<SkillHandler>();
        _skillHandler.Initialize(this);
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        Manager.Instance.DataManager.Heal(amount);
    }

    public override async Task Damage(int amount)
    {
        await base.Damage(amount);
        Manager.Instance.DataManager.Damage(amount);
    }

    public async Task DrawCard(int count, bool isFirstTurn)
    {
        await _deckHandler.DrawCard(count, true, this);
    }

    public async void UseCard(List<CharacterBase> targets, Card card)
    {
        if (IsControlled && card.Data.FileName != "Manipulation") return;

        BlockerManager.Instance.Active();
        var cardEffect = new CardEffectContext
        {
            Card = card,
            User = this,
            Targets = targets,
            Drawer = this
        };

        Cost.Use(card.Data.Cost);
        await _deckHandler.UseCard(card, cardEffect);
        Manager.Instance.AnalyticsManager.LogEvent(EventName.CARD_USED, EventParam.CARD_NAME, card.Data.CardName);
        BlockerManager.Instance.InActive();
    }

    public bool IsFullDeck()
    {
        return _deckHandler.IsFullDeck();
    }
    public Task UseCard()
    {
        return default;
    }

    public override async Task Dead()
    {
        _manager.GameOver();
        await Task.CompletedTask;
    }

    #region 유니크 스킬 사용자
    protected SkillHandler _skillHandler;

    public SkillSO SkillData => _skillData;
    public bool IsCanUseSkill { get; set; } = true;
    protected SkillSO _skillData;

    public abstract Task UseSkill();
    #endregion
}
