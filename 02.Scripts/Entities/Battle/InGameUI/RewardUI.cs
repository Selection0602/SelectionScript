using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [SerializeField] private Image _panel;
    [SerializeField] private TextMeshProUGUI _remainTimeText;
    [SerializeField] private RewardCell[] _rewardCellList;
    [SerializeField] private GameObject _bossReward;

    private DataManager _dataManager;
    private BattleDataManager _battleDataManager;
    private IReadOnlyDictionary<int, RewardData> _rewardDatas;

    private int _rewardCount = 3;
    private NodeType _node;
    private List<RewardData> _rewardList = new();
    
    private RandomEventController _randomEventController;

    [Header("연출")]
    [SerializeField] private int _selectDelay = 30; // 선택 대기 시간(초)
    [SerializeField] private int _showRewardDelay = 30; // 보상 표시 대기 시간 (60=1초)
    [SerializeField] private float _fadeInEndValue = 0.8f; // 페이드 인 목표 알파값
    [SerializeField] private float _fadeInDuration = 0.5f; // 페이드 인 목표 도달 시간

    private void Awake()
    {
        _dataManager = Manager.Instance.DataManager;
        
        if(SceneBase.Current is BattleSceneController battleSceneController)
            _battleDataManager = battleSceneController.BattleDataManager;
        else if(SceneBase.Current is RandomEventScene randomEventScene)
            _randomEventController = randomEventScene.EventController;
    }

    public async Task ShowRewardUI(NodeType nodeType)
    {
        _node = nodeType;

        //서서히 페이드인
        gameObject.SetActive(true);
        await _panel.DOFade(_fadeInEndValue, _fadeInDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        await UniTask.Delay(_showRewardDelay); //0.5초 대기
        await ShowReward();
    }

    private async Task ShowReward()
    {
        TaskCompletionSource<bool> rewardSelected = new();
        List<Task> tasks = new();

        if (_node == NodeType.BossBattle)
        {
            _bossReward.SetActive(true);
            await rewardSelected.Task;
        }
        else
        {
            SetRewardList(_node);
            var rewardList = await GetRandomRewardList(_rewardCount);
            for (int i = 0; i < rewardList.Count; i++)
            {
                var data = rewardList[i];
                var cell = _rewardCellList[i];
                cell.gameObject.SetActive(true);
                cell.Set(data);
                cell.OnSelectReward += () =>
                {
                    rewardSelected.TrySetResult(true); // Task 완료
                };
            }

            if (rewardList.First().TypeValue == RewardType.Memory)
            {
                var tutorialController = ServiceLocator.GetService<TutorialController>();
                if (tutorialController)
                    tutorialController.StartMemoryTutorial();
            }

            await Task.WhenAny(rewardSelected.Task, Countdown(_selectDelay));
        }
    }

    private async Task<List<RewardData>> GetRandomRewardList(int count)
    {
        if (_node == NodeType.BossBattle)
        {
            foreach (var reward in _rewardDatas.Values)
            {
                _rewardList.Add(reward);
            }
            return _rewardList;
        }

        for (int i = 0; i < count; i++)
        {
            var reward = await GetRandomReward();
            _rewardList.Add(reward);
        }
        return _rewardList;
    }

    private Task<RewardData> GetRandomReward()
    {
        int safety = 100;
        while (safety-- > 0)
        {
            var selected = RandomUtility.GetRandomValue(_rewardDatas);
            bool isPass = IsPassDrawReward(selected);
            if (isPass)
            {
                continue;
            }
            return Task.FromResult(selected);
        }
        return Task.FromResult(_rewardDatas[1]);
    }

    private bool IsPassDrawReward(RewardData data)
    {
        if (IsExistReward(data))
        {
            return true;
        }
        if(data.TypeValue == RewardType.Booty && _dataManager.IsExistBooty(data))
        {
            return true;
        }
        if (data.TypeValue == RewardType.Memory && _dataManager.Memories.ContainsKey(data.Index))
        {
            return true;
        }
        if (data.TypeValue == RewardType.Card && !_dataManager.IsCanAddCard(data.GetCardIndex))
        {
            return true;
        }
        return false;
    }

    private bool IsExistReward(RewardData data)
    {
        foreach(var reward in _rewardList)
        {
            if(reward.Index == data.Index)
            {
                return true;
            }
        }
        return false;
    }

    private void SetRewardList(NodeType node)
    {
        switch (node)
        {
            case NodeType.NormalBattle:
                _rewardDatas = _battleDataManager.RewardDatas;
                break;
            case NodeType.EliteBattle:
                _rewardDatas = _battleDataManager.EliteRewardDatas;
                break;
            case NodeType.RandomEvent:
                if(_randomEventController)
                    _rewardDatas = _randomEventController.RewardDict;
                break;
        }
    }

    private async Task Countdown(float duration)
    {
        float timeLeft = duration;
        _remainTimeText.gameObject.SetActive(true);

        while (timeLeft > 0f)
        {
            _remainTimeText.text = $"남은시간 : {Mathf.CeilToInt(timeLeft)}s";
            await UniTask.Delay(100); // 0.1초마다 업데이트
            timeLeft -= 0.1f;
        }

        _remainTimeText.text = "0";
    }
}
