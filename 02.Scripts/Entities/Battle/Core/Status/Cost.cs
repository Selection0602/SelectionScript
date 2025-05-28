using System;

public class Cost
{
    public Cost(int cost)
    {
        _maxCost = cost;
        _current = cost;
    }

    public int Current
    {
        get => _current;
        private set
        {
            _current = value;
            OnChangedCost?.Invoke(_current, _maxCost);
        }
    }

    public int Max => _maxCost;

    private int _maxCost;
    private int _current;

    public Action<int, int> OnChangedCost = delegate { };

    public void RecoveryCost()
    {
        Current = _maxCost;
    }

    public bool IsFullCost()
    {
        return _current == _maxCost;
    }

    public void Use(int cost)
    {
        Current = Math.Max(0, Current - cost);
    }

    public void Add(int cost)
    {
        Current = Math.Min(Current + cost, _maxCost);
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_GetCost);
    }

    public void IncreaseMaxCost(int cost)
    {
        _maxCost += cost;
        OnChangedCost?.Invoke(Current, _maxCost);
    }

    public void DecreaseMaxCost(int cost)
    {
        _maxCost -= cost;
        OnChangedCost?.Invoke(Current, _maxCost);
    }

    public void ModifyMaxCost(int cost)
    {
        _maxCost = cost;
        OnChangedCost?.Invoke(Current, _maxCost);
    }

    public bool IsCanUse(int cost)
    {
        return Current - cost >= 0;
    }
}
