using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyUnlockObject : BaseInteractObject
{
    public TalkList HaveKeyTList;                   //Key가 있을때 뜰 TalkList
    public TalkList DontHaveKeyTList;               //Key가 없을때 뜰 TalkList
    public Transform CheckObjectList;               //실패여부를 체크해야할 곳(2스테이지 CheckObject들)

    //질문 딕셔너리 bool
    public Dictionary<int, bool> TChecked = new Dictionary<int, bool>();

    public bool IsHaveKey = false;                  //Key 존재 여부

    public bool AllTrue;                            //2스테이지에서 전부 체크했는지의 여부(실패의 여부)

    public JumpScareController JSController;       //점프 스케어

    public UnityEvent TeleportToGround;             //지상으로 이동

    [SerializeField] private AvoidItemData _keyItemData;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private TalkBox _talkBox;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        //키를 가지고 있을 경우
        if(!IsHaveKey) TList = DontHaveKeyTList;
        else TList = HaveKeyTList; 

        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < TList.Talks.Count; i++)
        {
            TDict[i] = TList.Talks[i].Replace("\\n", "\n"); ;
        }

        CanInteract = true;
        _tCount = TDict.Count;
        _currentCount = 0;
        Talk = TDict[_currentCount];

        CollectDatas();         //데이터 수집 
        CheckIsAllTrue();       //실패 여부 체크
    }

    //다음 질문을 실행시켜주는 함수
    public override void DisplayNextTalk()
    {
        //질문이 끝나질 않을 경우
        if (!isTalkFinished())
        {
            _currentCount++;
            if (!isTalkFinished())
            {
                Talk = TDict[_currentCount];
                Panel.SetSentance(Talk);
            }
            //끝날 경우
            if (_currentCount + 1 >= _tCount)
            {
                if (IsHaveKey)
                {
                    UnlockDoor();
                    if(AllTrue) StartCoroutine(WaitForFifteenSeconds());
                    CanInteract = false;
                }
                else CanInteract = true;
            }
        }
    }

    //Key 상태 변경 : true
    public void GetKey()
    {
        _inventory.AddItem(_keyItemData);
        Panel.OnClosePanel += () => _talkBox.StartDialogue(_keyItemData.DialogueTexts, null, _keyItemData.ItemSprite);
        IsHaveKey = true;
    }

    //Key 상태 변경 : false
    public void UnlockDoor()
    {
        IsHaveKey = false;
    }

    //2스테이지에서 전부 체크했는지의 여부 판정
    public void CheckIsAllTrue()
    {
        AllTrue = true;
        foreach (var pair in TChecked)
        {
            if (!pair.Value)
            {
                AllTrue = false;
                break;
            }
        }
    }

    //체크해야할 조건들을 수집하기
    public void CollectDatas()
    {
        var found = CheckObjectList.GetComponentsInChildren<CheckObject>();
        List<CheckObject> _interactables = new List<CheckObject>(found);
        int index = 0;
        for (int i = 0; i < _interactables.Count; i++)
        {
            for (int j = 0; j < _interactables[i].CheckDict.Count; j++)
            {
                TChecked[index] = _interactables[i].CheckDict[j];
                index++;
            }
        }
    }

    //실패하면 이동하되 15초 후 실패가 뜨도록 설정
    private IEnumerator WaitForFifteenSeconds()
    {
        yield return new WaitForSeconds(15f);
        JSController.StartJumpScare(0.1f, 0.5f, 2f);
    }
}
