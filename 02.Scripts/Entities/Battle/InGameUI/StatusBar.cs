using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    private TextMeshProUGUI _hpAmount;
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _hpAmount = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetBar(int max, int current)
    {
        UpdateBar(max,current);
    }

    public void UpdateBar(int max,int current)
    {
        _slider.maxValue = max;
        _slider.value = current;
        UpdateText(max,current);
    }

    private void UpdateText(int max,int current)
    {
        _hpAmount.text = $"{current}/{max}";
    }
}
