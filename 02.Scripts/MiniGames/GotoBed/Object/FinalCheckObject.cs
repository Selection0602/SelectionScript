using UnityEngine;

public class FinalCheckObject : BaseInteractObject
{
    private CheckPanel _checkPanel;             //체크 패널
    public FinalCheckPanel FinalPanel;          //최종 점검 패널
    private BasePanel _originalPanel;           //원래 패널

    private void Start()
    {
        Init();
    }

    //초기화 작업
    public override void Init()
    {
        base.Init();

        //이 오브젝트는 채널을 2개 띄울수 있으므로 _originalPanel을 이용해 관리
        if (_originalPanel == null) _originalPanel = Panel;

        _checkPanel = (CheckPanel)_originalPanel;
        Panel = _checkPanel;
    }

    //다음 질문을 실행시켜주는 함수
    public override void DisplayNextTalk()
    {
        //질문이 끝나질 않을 경우 (더블 체크)
        if (!isTalkFinished())
        {
            _currentCount++;
            //질문이 끝난 경우
            if (isTalkFinished())
            {
                // FinalPanel 띄우기, 체크해야할 데이터 수집 
                Panel.ClosePanel();
                Panel = FinalPanel;
                FinalPanel.OpenPanel();
                FinalPanel.CollectDatas();
            }
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
