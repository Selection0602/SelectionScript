using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

public class RandomEventController : MonoBehaviour
{
    [SerializeField] private RandomEventUI _eventUI;
    [SerializeField] private RewardUI _rewardUI;

    private RandomEventBase _eventData;

    private Dictionary<int, RewardData> _rewardDict = new Dictionary<int, RewardData>();
    public IReadOnlyDictionary<int, RewardData> RewardDict => _rewardDict;

    private Dictionary<int, MemorySO> _memoryDict = new Dictionary<int, MemorySO>();
    public IReadOnlyDictionary<int, MemorySO> MemoryDict => _memoryDict; // RewardRegistry에서 사용하기 위해 메모리 SO 데이터 저장

    #region 이벤트 시작
    /// <summary>
    /// 랜덤 이벤트 시작 및 UI를 통해 이벤트 데이터 표시
    /// </summary>
    /// <param name="eventType">표시 할 이벤트 타입</param>
    public async void StartEvent(RandomEventType eventType)
    {
        try
        {
            // 어드레서블로 이벤트 데이터 가져오기
            await GetEventData(eventType);

            // 랜덤 보상 이벤트 일 경우 리워드 표시 함수 등록
            if (eventType is RandomEventType.RandomBooty or RandomEventType.RandomMemory)
                _eventData.OnEventStarted += HandleEventStarted;

            // SO 데이터 리워드 데이터로 변환 후 딕셔너리에 저장
            await GetRewardData(eventType);

            _eventUI.SetEvent(_eventData);
            _eventUI.StartFade();
        }
        catch (Exception e)
        {
            Debug.LogError($"보상 데이터 가져오는 중 오류 발생 {e.Message}");
        }
    }

    private async void HandleEventStarted()
    {
        try
        {
            // 버튼 클릭 시 보상 UI 표시
            await _rewardUI.ShowRewardUI(NodeType.RandomEvent);
            _eventUI.StartFade(false, 1f, () =>
            {
                SceneBase.GetCurrent<RandomEventScene>()?.MoveMapScene();
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"전리품 혹은 메모리 보상 선택 중 오류 발생; {e.Message}");
        }
    }
    #endregion

    #region 이벤트 데이터 가져오기
    private async Task GetEventData(RandomEventType eventType)
    {
        _eventData = await Manager.Instance.AddressableManager.Load<RandomEventBase>(eventType.ToString());
    }

    private async Task GetRewardData(RandomEventType eventType)
    {
        switch (eventType)
        {
            // 전리품 이벤트라면 Reward 중 전리품 타입 데이터만 가져오기
            case RandomEventType.RandomBooty:
                var rewardDataList = await Manager.Instance.AddressableManager.GetHandleResultList<RewardSO>("Reward");
                var bootyIcons = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("BootyIcons");
                var rewardImages = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("RewardImages");
                foreach (var data in rewardDataList)
                {
                    if (data.TypeValue == RewardType.Booty)
                    {
                        data.Image = rewardImages.GetSprite($"{data.FileName}");
                        data.Icon = bootyIcons.GetSprite($"{data.FileName}");
                        var reward = GetRewardData(data);
                        _rewardDict.Add(reward.Index, reward);
                    }
                }
                break;

            // 메모리 이벤트라면 메모리 중 보스 타입 제외한 데이터 가져오기
            case RandomEventType.RandomMemory:
                var memoryDataList = await Manager.Instance.AddressableManager.GetHandleResultList<MemorySO>("MemoryData");
                var memoryImages = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("MemoryImages");
                foreach (var data in memoryDataList)
                {
                    if (!data.IsBoss)
                    {
                        var reward = GetRewardData(data);
                        reward.Image = memoryImages.GetSprite($"{data.FileName}");
                        _rewardDict.Add(reward.Index, reward);

                        // RewardRegistry에서 메모리 SO 데이터 사용하기 위해 저장
                        _memoryDict.Add(reward.Index, data);
                    }
                }
                break;
        }
    }
    #endregion

    #region 보상 데이터 변환

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
            TypeValue = reward.TypeValue
        };
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

    #endregion

    private void OnDestroy()
    {
        if (_eventData)
            _eventData.OnEventStarted -= HandleEventStarted;
    }
}
