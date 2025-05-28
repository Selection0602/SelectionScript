using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private DropZone _playerDropZone;
    [SerializeField] private DropZone _globalDropZone;

    public InGameUI InGameUI;
    public RewardUI RewardUI;
    public OutlineEffect TurnEndEffect;

    public Player Player { get; set; }
    public Enemy LeaderEnemy { get; set; }
    public List<CharacterBase> CurrentAllies { get; set; } = new();
    public List<CharacterBase> CurrentEnemies { get; set; }
    public List<CharacterBase> DeadEnemies { get; set; } = new();
    public BattleSetup BattleSetup;

    private GameStateMachine _gameStateMachine;

    private bool isFirstTurn = true;
    public bool IsGameFinish { get; private set; } = false;

    public Action OnGameStart = delegate { };
    public Action OnGameFinish = delegate { };

    public NodeType Node { get; set; }

    public ICardDrawer EnemyDrawer { get; set; }
    public ICardDistributor EnemyDistributor { get; set; }

    //카드 뽑기 횟수
    public int PlayerDrawCount { get; set; } = 1;
    private int _enemyDrawCount { get; set; } = 1;

    public async void GameInitialize(NodeType type)
    {
        Node = type;

        _gameStateMachine = new GameStateMachine(this);

        //세팅된 캐릭터,적 가져오기
        BattleSetup.Initialize(this);

        Player = await BattleSetup.GetPlayer();
        InGameUI.InitializePlayerCost(Player.Cost);

        CurrentEnemies = await BattleSetup.GetEnemies(type);
        LeaderEnemy = CurrentEnemies[0] as Enemy;

        if (type == NodeType.BossBattle)
        {
            InGameUI.InitializeEnemyCost(LeaderEnemy.Cost);
        }
        await InGameUI.SetBattleUI(type);

        RegisterZoneEvent();

        Manager.Instance.DataManager.ApplyBootyEffects(this, Player);

        Player.Cost.OnChangedCost += ActiveTurnEndEffect;
        Player.DeckHandler.OnChangedHandCards += ActiveTurnEndEffect;

        _gameStateMachine.MoveNextState(GameState.GameStart);
    }

    private void ActiveTurnEndEffect(int current, int max)
    {
        ActiveTurnEndEffect(current);
    }

    private void ActiveTurnEndEffect(int current)
    {
        TurnEndEffect.gameObject.SetActive(Player.HandDeck.Count == 0 || Player.Cost.Current == 0);
    }

    public void RegisterZoneEvent()
    {
        _playerDropZone.OnDropCardEvent = UseCardOnSelf;
        _globalDropZone.OnDropCardEvent = UseCardAllTarget;
    }

    public void UseCardOnSelf(PlayerCard card, GameObject drop)
    {
        if (!card.IsCanUse)
        {
            return;
        }
        if (card.Data.TargetType == TargetType.Me || card.Data.EffectRange == EffectRange.NoneTarget)
        {
            card.CardAnimation.IsUseCard = true;
            Player.UseCard(CurrentAllies, card);
        }
        else
        {
            card.CardAnimation.IsUseCard = false;
            card.CardAnimation.OnEndDrag();
        }
    }
    public void UseCardAllTarget(PlayerCard card, GameObject drop)
    {
        if (!card.IsCanUse)
        {
            return;
        }
        if (card.Data.EffectRange == EffectRange.AllTarget || card.Data.EffectRange == EffectRange.NoneTarget)
        {
            card.CardAnimation.IsUseCard = true;
            Player.UseCard(GetIsAliveEnemies(), card);
        }
        else
        {
            card.CardAnimation.IsUseCard = false;
            card.CardAnimation.OnEndDrag();
        }
    }

    public async Task PlayerTurnStart()
    {
        if (isFirstTurn) return;
        var invoker = new CommandInvoker();
        invoker.Enqueue(new PlayerTurnStart(Player));
        invoker.Enqueue(new DrawCards(Player, PlayerDrawCount));
        await invoker.ExecuteAllAsync();
    }

    public void PlayerTurnEnd()
    {
        TurnEndEffect.gameObject.SetActive(false);
        Manager.Instance.AnalyticsManager.LogEvent(EventName.TURN_ENDED, EventParam.REMAINING_CARDS, Player.HandDeck.Count);
        _gameStateMachine.MoveNextState(GameState.PlayerTurnEnd);
    }

    public async Task EnemyTurnStart()
    {
        var invoker = new CommandInvoker();
        if (!isFirstTurn)
        {
            invoker.Enqueue(new DrawCards(EnemyDrawer, _enemyDrawCount));
        }
        invoker.Enqueue(new EnemyTurnStart(EnemyDistributor, GetIsAliveEnemies(), isFirstTurn));
        await invoker.ExecuteAllAsync();
    }

    public async UniTask EnemyAttack()
    {
        foreach (var enemy in GetIsAliveEnemies())
        {
            if (enemy is ICardUser user)
            {
                await user.UseCard();
            }
            if (enemy is IUniqueSkillUser skillUser)
            {
                await skillUser.UseSkill();
            }
            await UniTask.Delay(100);
        }
    }

    public List<CharacterBase> GetIsAliveEnemies()
    {
        var enemies = new List<CharacterBase>();
        foreach (var enemy in CurrentEnemies)
        {
            if (!enemy.IsDead)
            {
                enemies.Add(enemy);
            }
        }
        return enemies;
    }

    public async UniTask EnemyTurnEnd()
    {
        if (isFirstTurn)
        {
            isFirstTurn = false;
        }
        foreach (var enemy in GetIsAliveEnemies())
        {
            await enemy.OnTurnEnd();
        }
        await Player.OnDebuff();
        await UniTask.Delay(500);
    }

    public void OnEnemyDied(CharacterBase enemy)
    {
        DeadEnemies.Add(enemy);
        Player.OnKillEnemy?.Invoke();

        if (DeadEnemies.Count == CurrentEnemies.Count)
        {
            GameClear();
        }
    }

    public void GameOver()
    {
        Manager.Instance.AnalyticsManager.LogEvent(EventName.PLAYER_DEATH, EventParam.STAGE_NUMBER,
            Manager.Instance.MapManager.SavedMapData.PlayerNode.Layer);

        IsGameFinish = true;
        _gameStateMachine.MoveNextState(GameState.GameOver);
        MoveNextScene(false);
    }

    public void GameClear()
    {
        IsGameFinish = true;
        _gameStateMachine.MoveNextState(GameState.GameClear);
    }

    public void MoveNextScene(bool isClear)
    {
        (SceneBase.Current as BattleSceneController).MoveNextScene(isClear);
    }


}
