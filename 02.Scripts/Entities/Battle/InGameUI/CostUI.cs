using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class CostUI : MonoBehaviour
{
    private Image _costImage;
    private TextMeshProUGUI _costText;
    private Light2D _light2D;

    private void Awake()
    {
        _costImage = GetComponent<Image>();
        _costText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetImage(Sprite sprite)
    {
        _costImage.sprite = sprite;
    }

    public void Initialize(Cost cost, bool isBoss = false)
    {
        Action<int, int> updateCostEvent = isBoss ? UpdateCost : UpdateCost_Enemy;
        cost.OnChangedCost += updateCostEvent;
        updateCostEvent(cost.Current, cost.Max);
    }

    public void SetCostLight(Color color)
    {
        _light2D = GetComponentInChildren<Light2D>(true);
        _light2D.color = color;
        _light2D.gameObject.SetActive(true);
    }

    private void UpdateCost(int current, int max)
    {
        _costText.text = $"{current}/{max}";
        if (_light2D != null)
            _light2D.gameObject.SetActive(current != 0);
    }

    private void UpdateCost_Enemy(int current, int max)
    {
        _costText.text = $"{current}";
    }
}
