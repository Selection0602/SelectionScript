using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.U2D;

public class BattleDataManager : MonoBehaviour
{
    public async Task LoadBattleDatas()
    {
        await LoadDebuffDatas();  //디버프 데이터 로드
        await LoadMonsterDatas(); //몬스터 데이터 로드
        await LoadSkillDatas(); //스킬 데이터 로드
        await LoadMemoryDatas(); //메모리 데이터 로드
        await LoadRewardDatas(); //보상 데이터 로드
    }

    public async Task LoadBattleUI(NodeType node)
    {
        var bossUI = await Manager.Instance.AddressableManager.Load<GameObject>("BossUI");
        _bossUI = bossUI.GetComponent<StatusUI>();

        var enemyUI = await Manager.Instance.AddressableManager.Load<GameObject>("EnemyUI");
        _enemyUI = enemyUI.GetComponent<StatusUI>();
    }

    public async Task LoadAnimations()
    {
        foreach (var enemy in _enemyDatas.Values)
        {
            var handle = await Manager.Instance.AddressableManager.Load<GameObject>(enemy.FileName);
            var anim = handle.GetComponent<EnemyAnimController>();
            _enemyAnimations.Add(enemy.Index, anim);
        }
    }

    #region 배틀 UI 관련

    public CharacterTypeImageSO PlayerImageData => _playerImageData;
    private CharacterTypeImageSO _playerImageData;

    public CharacterTypeImageSO EnemyImageData => _enemyImageData;
    private CharacterTypeImageSO _enemyImageData;

    public async Task LoadPlayerImageDatas(string name)
    {
        _playerImageData = await Manager.Instance.AddressableManager.Load<CharacterTypeImageSO>(name);
    }

    public async Task LoadEnemyImageDatas(string name)
    {
        _enemyImageData = await Manager.Instance.AddressableManager.Load<CharacterTypeImageSO>(name);
    }

    public async Task<Sprite> LoadBackGround(string name)
    {
        var sprite = await Manager.Instance.AddressableManager.Load<Sprite>($"{name}BG");
        return sprite;
    }

    #endregion

    #region 디버프 관련 함수
    public IReadOnlyDictionary<DebuffType, DebuffSO> DebuffDatas => _debuffData;
    private Dictionary<DebuffType, DebuffSO> _debuffData = new();

    private async Task LoadDebuffDatas()
    {
        var debuffDatas = await Manager.Instance.AddressableManager.GetHandleResultList<DebuffSO>("Debuff");

        foreach (var data in debuffDatas)
        {
            _debuffData.Add(data.DebuffType, data);
        }
    }
    #endregion

    #region 몬스터 데이터
    public IReadOnlyDictionary<int, MonsterSO> EnemyDatas => _enemyDatas;
    private Dictionary<int, MonsterSO> _enemyDatas = new();

    public IReadOnlyDictionary<int, MonsterSO> NormalEnemyDatas => _normalEnemyDatas;
    private Dictionary<int, MonsterSO> _normalEnemyDatas = new();

    public IReadOnlyDictionary<int, MonsterSO> EliteEnemyDatas => _eliteEnemyDatas;
    private Dictionary<int, MonsterSO> _eliteEnemyDatas = new();

    public IReadOnlyDictionary<int, MonsterSO> BossDatas => _bossDatas;
    private Dictionary<int, MonsterSO> _bossDatas = new();

    public IReadOnlyDictionary<int, EnemyAnimController> EnemyAnimations => _enemyAnimations;
    private Dictionary<int, EnemyAnimController> _enemyAnimations = new();

    public MonsterSO SummonData => _summonData;
    private MonsterSO _summonData;

    public StatusUI EnemyUI => _enemyUI;
    private StatusUI _enemyUI;

    public StatusUI BossUI => _bossUI;
    private StatusUI _bossUI;

    private async Task LoadMonsterDatas()
    {
        var enemyDatas = await Manager.Instance.AddressableManager.GetHandleResultList<MonsterSO>("Monster");
        foreach (var data in enemyDatas)
        {
            _enemyDatas.Add(data.Index, data);
        }
        foreach (var data in _enemyDatas)
        {
            if (data.Value.MonsterType == MonsterType.Normal)
            {
                _normalEnemyDatas.Add(data.Key, data.Value);
            }
            else if (data.Value.MonsterType == MonsterType.Elite)
            {
                _eliteEnemyDatas.Add(data.Key, data.Value);
            }
            else if (data.Value.MonsterType == MonsterType.Boss)
            {
                _bossDatas.Add(data.Key, data.Value);
            }
        }

        _summonData = await Manager.Instance.AddressableManager.Load<MonsterSO>("Summon");
        _enemyDatas.Add(_summonData.Index, _summonData);
    }
    #endregion

    #region 스킬 데이터
    public IReadOnlyDictionary<int, SkillSO> SkillDatas => _skillDatas;
    private Dictionary<int, SkillSO> _skillDatas = new();
    private async Task LoadSkillDatas()
    {
        var skillDatas = await Manager.Instance.AddressableManager.GetHandleResultList<SkillSO>("Skill");

        foreach (var data in skillDatas)
        {
            _skillDatas.Add(data.Index, data);
        }
    }
    #endregion

    #region 보상 데이터
    public IReadOnlyDictionary<int, RewardData> RewardDatas => _rewardDatas;
    private Dictionary<int, RewardData> _rewardDatas = new();

    public IReadOnlyDictionary<int, RewardData> EliteRewardDatas => _eliteRewardDatas;
    private Dictionary<int, RewardData> _eliteRewardDatas = new();

    public IReadOnlyDictionary<int, RewardData> BossRewardDatas => _bossRewardDatas;
    private Dictionary<int, RewardData> _bossRewardDatas = new();

    private async Task LoadRewardDatas()
    {
        var bootyIcons = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("BootyIcons");
        var rewardImages = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("RewardImages");
        var rewardDatas = await Manager.Instance.AddressableManager.GetHandleResultList<RewardSO>("Reward");

        foreach (var data in rewardDatas)
        {
            if (data.TypeValue != RewardType.Card)
            {
                data.Image = rewardImages.GetSprite($"{data.FileName}");
            }

            if (data.TypeValue == RewardType.Booty)
            {
                data.Icon = bootyIcons.GetSprite($"{data.FileName}");
            }

            var reward = GetRewardData(data);
            _rewardDatas.Add(reward.Index, reward);
        }

        if (_memoryDatas != null)
        {
            foreach (var memory in _memoryDatas.Values)
            {
                var reward = GetRewardData(memory);
                if (memory.IsBoss)
                {
                    _bossRewardDatas.Add(reward.Index, reward);
                }
                else
                {
                    _eliteRewardDatas.Add(reward.Index, reward);
                }
            }
        }
    }

    private RewardData GetRewardData(MemorySO memory)
    {
        return new RewardData
        {
            FileName = memory.FileName,
            Image = memory.Image,
            RewardName = memory.MemoryName,
            Index = memory.Index,
            Desc = memory.Desc,
            AtkUpValue = memory.AtkValue,
            HealUpValue = memory.HealValue,
            TypeValue = RewardType.Memory
        };
    }

    private RewardData GetRewardData(RewardSO reward)
    {
        return new RewardData
        {
            FileName = reward.FileName,
            Image = reward.Image,
            Icon = reward.Icon,
            Index = reward.Index,
            RewardName = reward.RewardName,
            Desc = reward.Desc,
            AtkUpValue = reward.AtkUpValue,
            HealUpValue = reward.HealUpValue,
            MaxHealValue = reward.MaxHealValue,
            DrawCard = reward.DrawCard,
            CostDown = reward.CostDown,
            NotBurn = reward.NotBurn,
            NotPoison = reward.NotPoison,
            NotWeaken = reward.NotWeaken,
            GetCard = reward.GetCard,
            GetCardIndex = reward.GetCardIndex,
            TypeValue = reward.TypeValue,
            CardDrawIndex = reward.CardDrawIndex
        };
    }
    #endregion

    #region 메모리 데이터

    public IReadOnlyDictionary<int, MemorySO> MemoryDatas => _memoryDatas;
    private Dictionary<int, MemorySO> _memoryDatas = new();

    private async Task LoadMemoryDatas()
    {
        var memoryImages = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("MemoryImages");
        var memoryDatas = await Manager.Instance.AddressableManager.GetHandleResultList<MemorySO>("MemoryData");

        foreach (var data in memoryDatas)
        {
            data.Image = memoryImages.GetSprite($"{data.FileName}");
            _memoryDatas.Add(data.Index, data);
        }
    }
    #endregion
}
