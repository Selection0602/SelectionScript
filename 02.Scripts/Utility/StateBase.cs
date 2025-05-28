using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class StateBase<T>
{
    public StateMachine<T> StateMachine { get; set; }
    public abstract void OnEnter();
    public abstract void OnExit();
}

public partial class StateBase<T>
{
    public static StateBase<T> None { get; } = new State_None();
    private class State_None : StateBase<T>
    {
        public override void OnEnter() { }
        public override void OnExit() { }
    }
}
