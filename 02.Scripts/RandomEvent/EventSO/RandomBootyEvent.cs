using UnityEngine;

// 전리품 선택
[CreateAssetMenu(fileName = "RandomBootyEvent", menuName = "RandomEvents/RandomBootyEvent")]
public class RandomBootyEvent : RandomEventBase
{
    public override void ExecuteEvent()
    {
        RaiseOnEventStarted();
    }

    public override bool HasRequiredBooty() => true;
}
