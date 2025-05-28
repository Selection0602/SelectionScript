using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardCell : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _rewardName;
    [SerializeField] protected TextMeshProUGUI _rewardDesc;
    [SerializeField] protected Image _rewardIcon;
    [SerializeField] protected Button _rewardButton;
    [SerializeField] private TextMeshProUGUI _cost;
    [SerializeField] private Image _costImage;
    
    public Action OnSelectReward = delegate { };

    private RewardData _data;

    private void Start()
    {
        _rewardButton.onClick.AddListener(SelectReward);
    }

    private void SelectReward()
    {
        RewardEffectRegistry.RewardEffectMap[_data.TypeValue]?.Invoke(_data);

        string eventName = _data.TypeValue == RewardType.Memory
            ? EventName.ELITE_REWARD_SELECTED
            : EventName.NORMAL_REWARD_SELECTED;

        Manager.Instance.AnalyticsManager.LogEvent(eventName, EventParam.REWARD_NAME, _data.RewardName);

        OnSelectReward?.Invoke();
    }

    public void Set(RewardData rewardData)
    {
        _data = rewardData;

        if (rewardData.TypeValue == RewardType.Card)
        {
            var cardData = new CardData();

            foreach (var data in Manager.Instance.DataManager.CardDatas.Values)
            {
                if (data.Index == rewardData.GetCardIndex)
                {
                    cardData = data;
                    break;
                }
            }
            _costImage.gameObject.SetActive(true);
            _cost.text = $"{cardData.Cost}";
        }

        _rewardIcon.sprite = rewardData.Image;
        _rewardName.text = $"{rewardData.RewardName}";
        _rewardDesc.text = TextUtility.CleanLineBreaks(rewardData.Desc);
    }
}
