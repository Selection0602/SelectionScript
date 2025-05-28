using UnityEngine;

// 메모리 선택
[CreateAssetMenu(fileName = "RandomMemoryEvent", menuName = "RandomEvents/RandomMemoryEvent")]
public class RandomMemoryEvent : RandomEventBase
{
    public override void ExecuteEvent()
    {
        RaiseOnEventStarted();
    }

    public override bool HasRequiredBooty() => true;
}