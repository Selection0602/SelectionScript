using System;

public class AttackPower
{
    public AttackPower(int baseAttackPower, int bouns = 0)
    {
        _current = baseAttackPower + bouns;
        _attackLevel = _current == 0 ? _current = 0 : _current - 1;
    }

    private int _current;

    public int Current
    {
        get => _current;
        private set
        {
            _current = value;
            OnChangedAttackPower?.Invoke(_attackLevel);
        }
    }

    public Action<int> OnChangedAttackPower = delegate { };

    private int _attackLevel = 0;

    public void IncreaseAttackPower(int amount)
    {
        _attackLevel += amount;
        Current += amount;
    }

    public void DecreaseAttackPower(int amount)
    {
        _attackLevel -= amount;
        Current -= amount;
    }

    public int GetAttackLevel()
    {
        return _attackLevel;
    }
}