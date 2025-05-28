using System;
using System.Threading.Tasks;
using UnityEngine;

public class Debuff
{
    public Sprite TurnEffectIcon;
    public DebuffType Type;
    public int Duration; //몇턴 동안 지속되는지
    public Action OnDurationEnd;
    public Func<Task> OnTurnEnd;
    public bool IsInfinite = false;
    public event Action<int> OnChangedDuration = delegate { };
    public bool IsExpired => Duration <= 0 && !IsInfinite; 

    public async Task Tick()
    {
        if (OnTurnEnd != null)
        {
            await OnTurnEnd.Invoke();
        }
        if (!IsInfinite)
        {
            Duration--;
            if (Duration == 0)
            {
                OnDurationEnd?.Invoke();
            }
            OnChangedDuration?.Invoke(Duration);
        }
    }
}
