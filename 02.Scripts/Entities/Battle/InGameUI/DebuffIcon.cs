using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebuffIcon : MonoBehaviour
{
    [SerializeField] private Image _debuffIcon;
    [SerializeField] private TextMeshProUGUI _remainTurn;

    public Debuff Data
    {
        get => _debuff;
        set
        {
            _debuff = value;
            Set();
        }
    }
    private Debuff _debuff;

    private void Set()
    {
        _debuffIcon.gameObject.SetActive(true);
        _debuffIcon.sprite = _debuff.TurnEffectIcon;

        if (!_debuff.IsInfinite)
        {
            _remainTurn.gameObject.SetActive(true);
            _debuff.OnChangedDuration += UpdateDebuffIcon;
            UpdateDebuffIcon(_debuff.Duration);
        }
    }

    private void OnDisable()
    {
        if (_debuff != null && !_debuff.IsInfinite)
        {
            _debuff.OnChangedDuration -= UpdateDebuffIcon;
        }
    }

    private void UpdateDebuffIcon(int remainTurn)
    {
        if (remainTurn == 0)
        {
            _debuffIcon.gameObject.SetActive(false);
            _remainTurn.gameObject.SetActive(false);
        }
        else
        {
            _remainTurn.text = $"{remainTurn}";
        }
    }
}
