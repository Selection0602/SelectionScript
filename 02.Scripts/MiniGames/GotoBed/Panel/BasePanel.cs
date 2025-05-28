using System;
using TMPro;
using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    public TextMeshProUGUI sentance;        //Panel에 있는 Text
    public event Action OnClosePanel;

    private void Start()
    {
        Init();
    }

    //초기화
    public virtual void Init() { }

    //패널 열기
    public virtual void OpenPanel()
    {
        gameObject.SetActive(true);
    }
    
    //패널 닫기
    public virtual void ClosePanel()
    {
        OnClosePanel?.Invoke();
        gameObject.SetActive(false);
    }

    //패널 Text 설정
    public virtual void SetSentance(string talk)
    {
        sentance.text = talk;
    }

    private void OnDisable()
    {
        OnClosePanel = null;
    }
}
