using UnityEngine;
using UnityEngine.Events;

public class DoorEscapeObject : BaseInteractObject
{
    public CheckObject CO;                  //검사해야할 CheckObject (2스테이지의 LobbyDoor)

    [Header("Result TalkList Settings")]
    [SerializeField] private TalkList _tList_01;              //결과1 - 체크O && 칼X 
    [SerializeField] private TalkList _tList_02;              //결과2 - 체크O && 칼O
    [SerializeField] private TalkList _tList_03;              //결과3 - 체크X && 칼O

    [Header("Result Panel Settings")]
    [SerializeField] private NormalPanel _normalPanel;      //실패했을 때 뜰 패널
    [SerializeField] private CheckPanel _checkPanel;        //성공했을 때 뜰 패널
    private int _currentSituation = 0;                      //결과 상황
    private bool _isHaveKnife = false;                      //칼 가지고 있는지의 여부

    [Header("Game Clear Event")]
    public UnityEvent GameClear;                            //게임 클리어 조건을 만족시켰을때(문을 열때)
    
    private void Start()
    {
        _currentCount = 0;
        Init();
    }

    public override void Init()
    {
        //결과1 - 체크O && 칼X 
        if (CO.CheckDict[_currentCount] && !_isHaveKnife)
        {
            Panel = _normalPanel;
            TList = _tList_01;
            _currentSituation = 1;
        }
        //결과2 - 체크O && 칼O
        else if (CO.CheckDict[_currentCount] && _isHaveKnife)
        {
            Panel = _checkPanel;
            TList = _tList_02;
            _currentSituation = 2;
        }
        //결과3 - 체크X && 칼O
        else
        {
            Panel = _checkPanel;
            TList = _tList_03;
            _currentSituation = 3;
        }

        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < TList.Talks.Count; i++)
        {
            TDict[i] = TList.Talks[i].Replace("\\n", "\n"); ;
        }

        Talk = TDict[_currentCount];
        _tCount = TDict.Count;
        CanInteract = true;
    }

    //탈출시키는 함수
    public override void DisplayNextTalk()
    {
        if (_currentSituation == 1)     //체크O && 칼X 
        {
            return;
        }
        else if (_currentSituation == 2) GameClear?.Invoke();   //체크O && 칼O
        else GameClear?.Invoke();                               //체크X && 칼O
    }

    //칼 상태 true로 변환
    public void ChangeHavingKnife()
    {
        _isHaveKnife = true;
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