using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ILoadingView
{
    void SetText(TipSO text);
    void SetProgress(float progress);
    void Initialize();
}

public class LoadingUI : BaseUI, ILoadingView
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI tipText;

    public override void Initialize()
    {
        Hide();
        SetProgress(0);
    }

    public void SetText(TipSO text)
    {
        string tip = TextUtility.CleanLineBreaks(text.Desc);
        tipText.text = tip;
    }

    public void SetProgress(float progress)
    {
        progressBar.fillAmount = Mathf.Clamp01(progress);
    }
}