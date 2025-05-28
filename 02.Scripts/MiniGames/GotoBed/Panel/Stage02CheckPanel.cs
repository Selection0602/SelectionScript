using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Stage02CheckPanel : FinalCheckPanel, IPanelButton
{
    public TalkList FinalTalkList;          //질문지
    public GameObject ButtonObjects;        //버튼들이 있는 그룹

    //질문 딕셔너리
    public Dictionary<int, string> TDict = new Dictionary<int, string>();

    private int _tCount;                      //질문 개수
    private int _currentCount;                //현재 실행한 질문의 개수
    public string Talk;                       //현재 질문 text

    public Button YesButton;        //"예" 버튼
    public Button NoButton;         //"아니오" 버튼
    private Button _defaultButton;

    public Button DefaultButton { get { return _defaultButton; } set { _defaultButton = value; } }

    #region Unity Event
    [Header("최종 통과시")]
    public UnityEvent OnCheckSuccess;

    [Header("중단하고 초기화")]
    public UnityEvent OnInitialCheckObjects;

    [Header("중간검사시 버튼 비활성 후 조작 전환")]
    public UnityEvent OnButtonSetFalse;

    [Header("중간검사시 버튼 활성 후 조작 전환")]
    public UnityEvent OnButtonSetTrue;
    #endregion 

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < FinalTalkList.Talks.Count; i++)
        {
            TDict[i] = FinalTalkList.Talks[i];
        }
        DefaultButton = YesButton;
        _tCount = TDict.Count;
        _currentCount = 0;
        Talk = TDict[_currentCount];
        sentance.text = Talk;
    }

    //실시간으로 버튼 색상 변경
    private void Update()
    {
        ChangeButtonColor();
    }

    //IPanelButton
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
        OnButtonSetFalse.Invoke();

        yield return new WaitForSeconds(2f);

        ButtonObjects.SetActive(true);
        sentance.gameObject.SetActive(true);
        OnButtonSetTrue.Invoke();
    }

    //다음 질문 출력
    public void DisplayNextTalk()
    {
        StartCoroutine(HideButtons());

        //질문이 끝나질 않을 경우
        if (_currentCount != _tCount)
        {
            _currentCount++;
            if (_currentCount == _tCount)
            {
                Invoke("ClosePanel", 2f);
                Debug.Log("성공");
                OnCheckSuccess?.Invoke();
            }
            else
            {
                Talk = TDict[_currentCount];
                SetSentance(Talk);
            }
        }
    }

    //검증 대상들 초기화
    public void InitObjects()
    {
        ClosePanel();
        OnInitialCheckObjects?.Invoke();
    }
}
