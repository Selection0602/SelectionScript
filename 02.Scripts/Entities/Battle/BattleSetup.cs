using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BattleSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("보스 설정")]
    [SerializeField] private MonsterSO _debugBossData;

    [Header("일반 몬스터수")]
    [SerializeField] private int _monsterCount;
#endif

    [SerializeField] private GameObject playerObject;
    [SerializeField] private List<GameObject> _enemies;
    [SerializeField] private EnemyDeckHandler _deckHandler;
    [SerializeField] private EnemyPositions _enemyPositions;

    private BattleManager _battleManager;
    private BattleDataManager _battleDataManager;
    private DataManager _dataManager;
    private Queue<GameObject> _enemiesPool = new();
    private List<CharacterBase> _currentEnemies = new();
    private ICharacterFactory _characterFactory;
    private IEnemyFactory _enemyFactory;

    private EnemyCardCoordinator _enemyCoordinator;

    public void Initialize(BattleManager battleManager)
    {
        _battleManager = battleManager;
        _dataManager = Manager.Instance.DataManager;
        _battleDataManager = (SceneBase.Current as BattleSceneController).BattleDataManager;

        _characterFactory = new CharacterFactory(_battleDataManager);
        _enemyCoordinator = new EnemyCardCoordinator();

        foreach (var enemy in _enemies)
        {
            enemy.gameObject.SetActive(false);
            _enemiesPool.Enqueue(enemy);
        }
    }

    public async Task<Player> GetPlayer()
    {
        var player = await PlayerSetup();
        return player;
    }

    private async Task<Player> PlayerSetup()
    {
        var playerData = _dataManager.CharacterData;
#if UNITY_EDITOR
        if (playerData == null)
        {
            playerData = await Manager.Instance.AddressableManager.Load<CharacterSO>("2");
            await _dataManager.InitializeCharacter(playerData);
        }
#endif
        if (playerData != null)
        {
            var player = _characterFactory.CreateCharacter(playerObject, playerData);
            await _battleDataManager.LoadPlayerImageDatas(playerData.FileName);
            _battleManager.CurrentAllies.Add(player);
            return player;
        }
        return null;
    }

    public async Task<List<CharacterBase>> GetEnemies(NodeType node)
    {
        switch (node)
        {
            case NodeType.NormalBattle:
                await NormalEnemiesSetup();
                break;
            case NodeType.EliteBattle:
                await EliteSetup();
                break;
            case NodeType.BossBattle:
                await BossSetup();
                break;
        }
        _battleManager.EnemyDistributor = _enemyCoordinator;

        return _currentEnemies;
    }

    private async Task NormalEnemiesSetup()
    {
        await _battleDataManager.LoadEnemyImageDatas("Monster");
        _enemyFactory = new EnemyFactory();
        int enemyCount = GetEnemyCount(); //1~4마리 랜덤
        var enemyList = GetNormalEnemyList();

        var positions = _enemyPositions.GetPositions(enemyCount);

        _enemyCoordinator.SetDeck(_deckHandler, enemyList[0].CardIndex);
        _battleManager.EnemyDrawer = _enemyCoordinator;

        for (int i = 0; i < enemyCount; i++)
        {
            var enemyData = RandomUtility.GetRandomInList(enemyList);

            if (enemyData != null)
            {
                var enemyObject = _enemiesPool.Dequeue();
                enemyObject.gameObject.SetActive(true);
                enemyObject.transform.position = positions[i];
                var anim = Instantiate(_battleDataManager.EnemyAnimations[enemyData.Index], enemyObject.transform);
                var enemyUI = Instantiate(_battleDataManager.EnemyUI, enemyObject.transform);
                var enemy = _enemyFactory.CreateEnemy(enemyObject, enemyData, _deckHandler);
                _currentEnemies.Add(enemy);
                Debug.Log($"{enemyData.Name} 데이터 세팅!");
            }
        }
    }

    private int GetEnemyCount()
    {
#if UNITY_EDITOR
        if (_monsterCount != 0)
        {
            return _monsterCount;
        }
        return RandomUtility.GetRandomIndex(1, 4);
#else
        return RandomUtility.GetRandomIndex(1, 4);
#endif
    }

    private List<MonsterSO> GetNormalEnemyList()
    {
        //Normal 타입인 몬스터 데이터 불러오기
        var enemyData = _battleDataManager.NormalEnemyDatas;

        //카드 인덱스가 같은 몬스터끼리 그룹화
        Dictionary<int, List<MonsterSO>> grouped = new();
        foreach (var monster in enemyData.Values)
        {
            int index = monster.CardIndex;
            if (!grouped.ContainsKey(index))
            {
                grouped[index] = new List<MonsterSO>();
            }
            grouped[index].Add(monster);
        }
        int randomIndex = RandomUtility.GetRandomIndex(1, grouped.Count + 1);
        return grouped[randomIndex];
    }

    private async Task EliteSetup()
    {
        await _battleDataManager.LoadEnemyImageDatas("Monster");
        _enemyFactory = new EnemyFactory();
        var enemyData = RandomUtility.GetRandomValue(_battleDataManager.EliteEnemyDatas);
        _enemyCoordinator.SetDeck(_deckHandler, enemyData.CardIndex);
        _battleManager.EnemyDrawer = _enemyCoordinator;
        if (enemyData != null)
        {
            var enemyObject = _enemiesPool.Dequeue();
            enemyObject.gameObject.SetActive(true);
            enemyObject.transform.position = _enemyPositions.GetCenterPos();
            var anim = Instantiate(_battleDataManager.EnemyAnimations[enemyData.Index], enemyObject.transform);
            var enemyUI = Instantiate(_battleDataManager.EnemyUI, enemyObject.transform);
            var elite = _enemyFactory.CreateEnemy(enemyObject, enemyData, _deckHandler);
            _currentEnemies.Add(elite);
            Debug.Log($"{enemyData.Name} 데이터 세팅!");
        }
    }

    private async Task BossSetup()
    {
        var enemyData = GetBossData();
        await _battleDataManager.LoadEnemyImageDatas(enemyData.FileName);
        _enemyFactory = new BossFactory(_battleDataManager);
        _enemyCoordinator.SetDeck(_deckHandler, enemyData.CardIndex);

        if (enemyData != null)
        {
            var enemyObject = _enemiesPool.Dequeue();
            enemyObject.gameObject.SetActive(true);
            enemyObject.transform.position = _enemyPositions.GetCenterPos();

            var anim = Instantiate(_battleDataManager.EnemyAnimations[enemyData.Index], enemyObject.transform);
            var enemyUI = Instantiate(_battleDataManager.BossUI, enemyObject.transform);
            var boss = _enemyFactory.CreateEnemy(enemyObject, enemyData, _deckHandler);
            _battleManager.EnemyDrawer = boss as ICardDrawer;
            _currentEnemies.Add(boss);
            Debug.Log($"{enemyData.Name} 데이터 세팅!");
        }
    }

    private MonsterSO GetBossData()
    {
#if UNITY_EDITOR
        if (_debugBossData != null)
        {
            return _debugBossData;
        }
        return RandomUtility.GetRandomValue(_battleDataManager.BossDatas);
#else
       return RandomUtility.GetRandomValue(_battleDataManager.BossDatas);
#endif
    }

    public Summon GetSummon(int num)
    {
        //소환수 세팅
        var enemyObject = _enemiesPool.Dequeue();
        enemyObject.gameObject.SetActive(true);
        enemyObject.transform.position = _enemyPositions.GetSummonPosition(num);

        var summon = enemyObject.AddComponent<Summon>();
        var summonData = _battleDataManager.SummonData;
        var battleCharacterData = CreateBattleEnemyData(summonData);

        var anim = Instantiate(_battleDataManager.EnemyAnimations[summonData.Index], enemyObject.transform);
        var enemyUI = Instantiate(_battleDataManager.EnemyUI, enemyObject.transform);
        summon.Initialize(battleCharacterData);
        _currentEnemies.Add(summon);
        return summon;
    }

    public BattleCharacterData CreateBattleEnemyData(MonsterSO data)
    {
        return new BattleCharacterData
        {
            Sprite = data.Image,
            MaxSantity = data.Health,
            CurrentSantity = data.Health,
            BaseAttackPower = data.Power,
            CardIndex = data.CardIndex,
            Cost = data.Cost,
        };
    }
}

