using System.Collections.Generic;
using UnityEngine;

public class CheckObject : BaseInteractObject
{       
    public Dictionary<int, bool> CheckDict = new Dictionary<int, bool>();   //체크 리스트 
    protected CheckPanel _checkPanel;                                       //체크 패널

    private void Awake()
    {
        Init();
    }

    //초기화 작업
    public override void Init()
    {
        base.Init();
        CheckDict = new Dictionary<int, bool>();
        _checkPanel = (CheckPanel)Panel;
        for (int i = 0; i < _tCount; i++)
        {
            CheckDict.Add(i, false);
        }
    }

    //다음 글 보여주기
    public override void DisplayNextTalk()
    {
        //질문이 끝나질 않을 경우 (더블 체크)
        if (!isTalkFinished())
        {
            CheckDict[_currentCount] = true;
            _currentCount++;

            //질문이 끝났을 경우 
            if (isTalkFinished())
            {
                CanInteract = false;
            }
            //질문이 끝나지 않았을 경우 다음 Talk 대입
            else Talk = TDict[_currentCount];
        }
    }

    //패널의 버튼에 이벤트 넘겨줌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<GBInteractDetector>() != null)
        {
            _checkPanel.YesButton.onClick.AddListener(DisplayNextTalk);
        }
    }

    //패널의 버튼에 이벤트 해제시켜줌
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<GBInteractDetector>() != null)
        {
            _checkPanel.YesButton.onClick.RemoveListener(DisplayNextTalk);
        }
    }
}
