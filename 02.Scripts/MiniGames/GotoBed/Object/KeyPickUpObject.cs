using UnityEngine;
using UnityEngine.Events;

public class KeyPickUpObject : BaseInteractObject
{
    [Header("Key를 얻는 이벤트")]
    public UnityEvent GetKey;

    private void Start()
    {
        base.Init();
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
            //0부터 시작하므로 1을 더해서 판별
            if (_currentCount + 1 >= _tCount)
            {
                GetKey?.Invoke();
                CanInteract = false;
                Panel.ClosePanel();
            }
        }
    }
}
