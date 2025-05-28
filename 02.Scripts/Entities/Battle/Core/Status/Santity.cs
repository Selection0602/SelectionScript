using System;
using UnityEngine;

public class Sanity
{
    public Sanity(int max, int current)
    {
        _maxSanity = max;
        CurrentSanity = current;
    }

    public int MaxSanity => _maxSanity;
    private int _maxSanity;

    public int CurrentSanity
    {
        get => _currentSanity;
        private set
        {
            _currentSanity = value;
            OnChangedCurrentSanity?.Invoke(_maxSanity,_currentSanity);
        }
    }
    private int _currentSanity;

    public Action<int,int> OnChangedCurrentSanity = delegate { };

    public void Heal(int amount)
    {
        CurrentSanity = Mathf.Min(_maxSanity, CurrentSanity + amount);
    }

    public void Damage(int amount)
    {
        CurrentSanity = Mathf.Max(0, CurrentSanity - amount);
    }

    public void IncreaseMaxSanity(int amount)
    {
        _maxSanity += amount;
        OnChangedCurrentSanity?.Invoke(_maxSanity, CurrentSanity);
    }

    public void DecreaseMaxSanity(int amount)
    {
        _maxSanity -= amount;
        OnChangedCurrentSanity?.Invoke(_maxSanity, CurrentSanity);
    }

    public bool IsFullSanity()
    {
        return _currentSanity == _maxSanity;
    }
}
