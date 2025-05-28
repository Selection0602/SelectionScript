using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInteractObject : MonoBehaviour
{
    public TalkList TList;                      //임시로 SO로 받은 질문 리스트
    public Dictionary<int, string> TDict = new Dictionary<int, string>();       //질문 딕셔너리  

    public string Talk;                         //현재 질문 text
    public bool CanInteract;                    //상호작용 가능 여부
    protected int _tCount;                      //질문 개수
    protected int _currentCount;                //현재 실행한 질문의 개수
    public BasePanel Panel;                     //각 Interactable에 맞는 패널

    private void Start()
    {
        Init();
    }

    //초기화 작업
    public virtual void Init()
    {
        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < TList.Talks.Count; i++)
        {
            TDict[i] = TList.Talks[i].Replace("\\n", "\n"); ;
        }

        //각 값들 초기 설정
        CanInteract = true;
        _tCount = TDict.Count;
        _currentCount = 0;
        Talk = TDict[_currentCount];
    }

    //다음 질문을 실행시켜주는 함수
    public virtual void DisplayNextTalk()
    {
        //질문이 끝나질 않을 경우 (더블 체크)
        if (!isTalkFinished())
        {
            _currentCount++;
            //질문이 끝나질 않을 경우
            if (!isTalkFinished())
            {
                Talk = TDict[_currentCount];
                Panel.SetSentance(Talk);
            }
            //0부터 시작하므로 1을 더해서 판별
            if (_currentCount + 1 >= _tCount)
            {
                CanInteract = false;
            }
        }
    }

    //총 질문 개수와 현재 질문 수가 일치하는지
    public virtual bool isTalkFinished()
    {
        return _currentCount == _tCount;
    }

    //질문의 처음을 체크하여 다음에 바로 대사가 없어져야 하는지를 판별
    public void FirstCheck()
    {
        if (_currentCount + 1 >= _tCount)
        {
            CanInteract = false;
        }
    }
}
