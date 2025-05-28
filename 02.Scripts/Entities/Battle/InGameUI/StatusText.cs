using TMPro;
using UnityEngine;

public class StatusText : MonoBehaviour
{
    private TextMeshProUGUI _statusText;

    private void Awake()
    {
        _statusText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateText(int amount)
    {
        bool isShow = amount != 0;
        transform.gameObject.SetActive(isShow);
        if (isShow)
        {
            _statusText.text = $"{amount}";
        }
    }
}
