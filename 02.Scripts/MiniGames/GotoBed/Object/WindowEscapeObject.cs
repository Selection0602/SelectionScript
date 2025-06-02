using UnityEngine;
using UnityEngine.Events;

public class WindowEscapeObject : BaseInteractObject
{
    public CheckObject CO;                  //검사해야할 CheckObject(스테이지 2의 Window)

    [Header("Result TalkList Settings")]
    [SerializeField] private TalkList _tList_01;              //결과1 - 열려있는 창문으로 탈출가능 
    [SerializeField] private TalkList _tList_02;              //결과2 - 창문을 열고 탈출가능
    [SerializeField] private TalkList _tList_03;              //결과3 -   창문을 열지 못함

    [Header("Result Panel Settings")]
    [SerializeField] private NormalPanel _normalPanel;      //실패했을 때 뜰 패널
    [SerializeField] private CheckPanel _checkPanel;        //성공했을 때 뜰 패널
    private int _currentSituation = 0;                      //결과 상황

    [Header("Game Clear Event")]
    public UnityEvent GameClear;                            

    private void Start()
    {
        _currentCount = 0;
        Init();
    }

    public override void Init()
    {
        _currentCount = 0;
        
        if (!CO.CheckDict[_currentCount])  //문 닫기 체크 X
        {
            Panel = _checkPanel;
            TList = _tList_01;
            _currentSituation = 1;
        }
        else  //문 닫기 체크 O
        {
            _currentCount++;

            if (!CO.CheckDict[_currentCount])   //문 닫기 체크 O && 문 잠금 체크 X
            {
                Panel = _checkPanel;
                TList = _tList_02;
                _currentSituation = 2;
            }
            else                               //문 닫기 체크 O &&  문 잠금 체크 O
            {
                Panel = _normalPanel;
                TList = _tList_03;
                _currentSituation = 3;
            }
        }
        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < TList.Talks.Count; i++)
        {
            TDict[i] = TList.Talks[i].Replace("\\n", "\n"); ;
        }

        _currentCount = 0;

        Talk = TDict[_currentCount];
        _tCount = TDict.Count;
        CanInteract = true;
    }

    //탈출시키는 함수
    public override void DisplayNextTalk()
    {
        //문 닫기 체크 X
        if (_currentSituation == 1)
        {
            GameClear?.Invoke();
        }
        else if (_currentSituation == 2) GameClear?.Invoke();       //문 닫기 체크 O && 문 잠금 체크 X
        else
        {
            base.DisplayNextTalk();
        }//문 닫기 체크 O && 문 잠금 체크 O

    }

    #region OnTrigger
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
    #endregion
}
