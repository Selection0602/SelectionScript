using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    private StatusText _statusText;
    private StatusBar _statusBar;

    private void Awake()
    {
        _statusBar = GetComponentInChildren<StatusBar>();
        _statusText = GetComponentInChildren<StatusText>();
    }

    public void SetBar(int max, int current)
    {
        UpdateBar(max, current);
    }

    public void UpdateBar(int max, int current)
    {
        _statusBar.UpdateBar(max, current);
    }

    public void UpdateAttackPowerText(int amount)
    {
        _statusText.UpdateText(amount);
    }
}
