using System.Collections.Generic;

public abstract class StateMachine<T>
{
    public StateMachine()
    {
        StateDic = CreateStateDic();
    }

    public StateBase<T> Current
    {
        get => current;
        set
        {
            current?.OnExit();
            current = value;
            current.StateMachine = this;
            current.OnEnter();
        }
    }

    private StateBase<T> current = StateBase<T>.None;

    public IReadOnlyDictionary<T, StateBase<T>> StateDic;  //상태가 담긴 딕셔너리

    protected abstract Dictionary<T, StateBase<T>> CreateStateDic(); //상태 클래스 생성

    //다음 상태로 전환
    public virtual void MoveNextState(T type)
    {
        Current = StateDic[type];
    }
}