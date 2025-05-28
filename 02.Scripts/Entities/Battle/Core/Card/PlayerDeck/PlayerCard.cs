using System.Threading.Tasks;
using UnityEngine;

public class PlayerCard : Card
{ 
    [SerializeField] private UIOutline _outline;

    private Cost _playerCost;

    protected override void Set()
    {
        _frontImage.sprite = _battleDataManager.PlayerImageData.CardFrontImage;
        base.Set();
        _playerCost = (SceneBase.Current as BattleSceneController).BattleManager.Player.Cost;
        _playerCost.OnChangedCost += UpdateUseableState;
        UpdateUseableState();
    }

    private void OnDisable()
    {
        if (_playerCost != null)
        {
            _playerCost.OnChangedCost -= UpdateUseableState;
        }
    }

    public override async Task Use()
    {
        _outline.gameObject.SetActive(false);
        _cost.gameObject.SetActive(false);
        _cardDesc.gameObject.SetActive(false);
        _cardName.gameObject.SetActive(false);

        await Dissolve(0.0f, 1.0f, 0.5f);
        CardAnimation?.ResetData();
    }

    private void UpdateUseableState(int current = 0, int max = 0)
    {
        if (!IsCanUse)
        {
            _cost.text = $"<color=red>{Data.Cost}</color>";
        }
        else
        {
            _cost.text = $"<color=black>{Data.Cost}</color>";
        }
        _outline?.gameObject.SetActive(IsCanUse);
    }

    public bool IsCanUse => _playerCost.Current - Data.Cost >= 0;
}
