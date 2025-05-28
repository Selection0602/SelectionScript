public class NormalObject : BaseInteractObject
{
    private void Start()
    {
        base.Init();
    }

    //다음 질문을 실행시켜주는 함수
    public override void DisplayNextTalk()
    {
        //질문이 끝나질 않을 경우 (더블 체크)
        if (!isTalkFinished())
        {
            _currentCount++;
            if (!isTalkFinished())  //질문이 끝나질 않을 경우
            {
                Talk = TDict[_currentCount];
                Panel.SetSentance(Talk);
            }
            if (_currentCount + 1 >= _tCount)   //0부터 시작하므로 1을 더해서 판별
            {
                CanInteract = false;
            }
        }
    }

}
