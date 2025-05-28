using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class Enemy : CharacterBase, ICardUser
{
    public CharacterBase Character => this;
    protected EnemyAnimController _animController;
    private MonsterDropZone _monsterDropZone;

    private CostUI _costUI;

    public override void Initialize(BattleCharacterData data)
    {
        _animController = GetComponentInChildren<EnemyAnimController>();
        _characterImage = _animController.GetComponent<Image>();

        base.Initialize(data);
        Cost = new Cost(data.Cost);
        _monsterDropZone = GetComponentInChildren<MonsterDropZone>(true);
        _monsterDropZone.Initialize(this);
        _effectPos = _monsterDropZone.transform;
    }

    public void SetCostUI()
    {
        _costUI = GetComponentInChildren<CostUI>(true);
        _costUI.gameObject.SetActive(true);
        _costUI.Initialize(Cost);
    }

    public override async Task Dead()
    {
        _attackPowerText.gameObject.SetActive(false);
        _costUI?.gameObject.SetActive(false);
        await _animController.PlayDeathAnim();
        _manager.OnEnemyDied(this);
        gameObject.SetActive(false);
    }

    #region 카드 사용자
    public List<Card> HandDeck => _sharedCards;
    public Dictionary<string, CardData> Decks => _decks;
    public DeckHandler DeckHandler => _deckHandler;
    public Cost Cost { get; set; }

    protected List<Card> _sharedCards = new();
    protected Dictionary<string, CardData> _decks = new();
    protected EnemyDeckHandler _deckHandler;

    public virtual ICardUseCondition UseCondition => new AllCardUseCondition(_defaultUseConditions);

    protected List<ICardUseCondition> _defaultUseConditions => new List<ICardUseCondition>()
    {
            new CostCondition(),
            new HealCondition(),
            new CleanseCondition(),
            new UseExchangeCondition(),
            new UseHealCostCondition(this),
     };

    protected ICardUseCondition CombineConditions(List<ICardUseCondition> additionalConditions)
    {
        var list = new List<ICardUseCondition>();
        list.AddRange(_defaultUseConditions);
        list.AddRange(additionalConditions);
        return new AllCardUseCondition(list);
    }

    public void RecoveryCost()
    {
        Cost.RecoveryCost();
    }

    public void ReceiveCards(List<Card> datas)
    {
        _sharedCards = datas;
    }

    public void SetDeckHandler(EnemyDeckHandler handler)
    {
        _deckHandler = handler;

        foreach (var data in handler.CardDatas)
        {
            _decks.Add(data.Key, data.Value);
        }
    }

    public virtual async Task PlayAttackAnim()
    {
        await _animController.PlayAttackAnim();
    }

    public async Task UseCard()
    {
        var cardEffect = new CardEffectContext
        {
            User = this,
            Targets = _manager.CurrentAllies,
            Drawer = _manager.EnemyDrawer
        };

        foreach (var card in _sharedCards)
        {
            Cost.Use(card.Data.Cost);
            await _deckHandler.UseCard(card, cardEffect);
            await UniTask.Delay(100);
        }
    }
    #endregion
}
