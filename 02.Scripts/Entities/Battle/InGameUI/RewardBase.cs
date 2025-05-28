using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class RewardCellBase : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _rewardName;
    [SerializeField] protected TextMeshProUGUI _rewardDesc;
    [SerializeField] protected Image _rewardIcon;
    [SerializeField] protected Image _backImage;
    [SerializeField] protected Button _rewardButton;

    public Action OnSelectReward = delegate { };

    private void Start()
    {
        _rewardButton.onClick.AddListener(SelectReward);
    }

    public abstract void Set(RewardSO data);

    protected virtual void SelectReward()
    {
        OnSelectReward?.Invoke();
    }
}