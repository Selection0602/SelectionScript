using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Stage01CheckPanel : FinalCheckPanel, IPanelButton
{
    public TalkList FinalTalkList;          //질문지
    public GameObject ButtonObjects;        //버튼들이 있는 그룹
    public Transform CheckObjectList;       //검증해야 하는 곳의 Transform

    //질문 딕셔너리
    public Dictionary<int, string> TDict = new Dictionary<int, string>();
    //질문 딕셔너리 bool
    public Dictionary<int, bool> TChecked = new Dictionary<int, bool>();
    public string Talk;                       //현재 질문 text

    private int _tCount;                      //질문 개수
    private int _currentCount;                //현재 실행한 질문의 개수

    public Button YesButton;                //"예" 버튼
    public Button NoButton;                 //"아니오" 버튼
    private Button _defaultButton;          //기본 버튼

    public Button DefaultButton { get { return _defaultButton; } set { _defaultButton = value; } }

    public JumpScareController JSController;

    [Header("최종 통과시")]
    public UnityEvent OnCheckSuccess;

    [Header("중간검사시 버튼 비활성 후 조작 전환")]
    public UnityEvent ButtonSetFalse;

    [Header("중간검사시 버튼 활성 후 조작 전환")]
    public UnityEvent ButtonSetTrue;

    private void Awake()
    {
        Init(); 
        DefaultButton = YesButton;
    }

    //실시간으로 버튼 색상 변경
    private void Update()
    {
        ChangeButtonColor();
    }

    //검증해야할 부분들 현재 상황 받아오기
    public override void Init()
    {
        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < FinalTalkList.Talks.Count; i++)
        {
            TDict[i] = FinalTalkList.Talks[i];
        }

        _tCount = TDict.Count;
        _currentCount = 0;
        Talk = TDict[_currentCount];
        sentance.text = Talk;
        
        CanCheck = true;
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
    }

    //버튼 색상 변경
    public void ChangeButtonColor()
    {
        if (DefaultButton == YesButton)
        {
            YesButton.image.color = Color.red;
            NoButton.image.color = Color.white;
        }
        else
        {
            YesButton.image.color = Color.white;
            NoButton.image.color = Color.red;
        }
    }

    //다음 질문을 실행시켜주는 함수
    public void DisplayNextTalk()
    {
        StartCoroutine(HideButtons());

        //질문이 끝나질 않을 경우
        if (TChecked[_currentCount] && _currentCount != _tCount)
        {
            _currentCount++;
            //최종통과시
            if (_currentCount == _tCount)
            {
                Invoke("ClosePanel", 2f);
                Debug.Log("성공");
                OnCheckSuccess?.Invoke();        
            }
            //질문이 남았을 경우
            else
            {
                Talk = TDict[_currentCount];
                SetSentance(Talk);
            }
        }
        //검증 실패시
        else
        {
            this.ClosePanel();
            JSController.StartJumpScare(0.1f, 0.5f, 2f);
        }
    }

    //패널 열기(딜레이)
    public override void OpenPanel()
    {
        base.OpenPanel();
        StartCoroutine(HideButtons());
    }

    //버튼 숨기고 다시 생기게 하는 코루틴 
    public IEnumerator HideButtons()
    {
        ButtonObjects.SetActive(false);
        sentance.gameObject.SetActive(false);
        ButtonSetFalse.Invoke();

        yield return new WaitForSeconds(2f);

        ButtonObjects.SetActive(true);
        sentance.gameObject.SetActive(true);
        ButtonSetTrue.Invoke();
    }

    //체크해야할 조건들을 수집하기
    public override void CollectDatas()
    {
        var found = CheckObjectList.GetComponentsInChildren<CheckObject>();
        List<CheckObject> _interactables = new List<CheckObject>(found);
        int index = 0;
        for(int i  = 0; i < _interactables.Count; i++) 
        {
            for (int j = 0; j < _interactables[i].CheckDict.Count; j++)
            {
                TChecked[index] = _interactables[i].CheckDict[j];
                index++;
            }
        }
    }
}
