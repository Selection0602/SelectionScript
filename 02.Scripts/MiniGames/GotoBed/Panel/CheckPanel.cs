using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckPanel : BasePanel, IPanelButton
{
    public Button YesButton;            //"예" 버튼
    public Button NoButton;             //"아니오" 버튼
    private Button _defaultButton;      //기본 버튼

    public Button DefaultButton { get { return _defaultButton; } set { _defaultButton = value; } }
    [SerializeField] private GameObject _selectCursor; //선택 커서

    private void Awake()
    {
        Init();
    }

    //기본 버튼 설정
    public override void Init()
    {
        base.Init();
        DefaultButton = YesButton;
        
        UpdateCursor();
    }

    private void OnEnable()
    {
        DefaultButton = YesButton;
        UpdateCursor();
    }

    //버튼 바꾸기
    public void ChangeButton()
    {
        if (DefaultButton == YesButton)
        {
            DefaultButton = NoButton;
        }
        else
        {
            DefaultButton = YesButton;
        }

        UpdateCursor();
    }
    
    private void UpdateCursor()
    {
        _selectCursor.transform.position = new Vector3(_selectCursor.transform.position.x, DefaultButton.transform.position.y, _selectCursor.transform.position.z);
    }
}
