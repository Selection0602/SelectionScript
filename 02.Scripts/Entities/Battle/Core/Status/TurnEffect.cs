using System;
using UnityEngine;

public class TurnEffect
{
    public Sprite TurnEffectIcon;
    public Action OnTurnStart;
    public Action OnTurnEnd;
    public Action OnDelayEnd;
    public int Duration = 0;
    public int Delay = 0;
    public bool IsInfinite = false;

    public event Action<int> OnChangedDuration = delegate { };

    public void StartTurnEffect()
    {
        if (Delay > 0) return;
        OnTurnStart?.Invoke();
    }

    public void EndTurnEffect()
    {
        if (Delay > 0)
        {
            Delay--; // 턴 종료 시 딜레이 감소
            if (Delay == 0)
            {
                OnDelayEnd?.Invoke();               
            }
            OnChangedDuration?.Invoke(Delay);
            return;
        }
        OnTurnEnd?.Invoke();
        if (!IsInfinite)
        {
            Duration--;
            OnChangedDuration?.Invoke(Duration);
        }
    }

    public bool IsExpired => Delay <= 0 && Duration <= 0 && !IsInfinite;
}
