using System;
using System.Threading.Tasks;

public class UseBuffCard : ICommand
{
    private BuffType _buffType;
    private CharacterBase _target;
    private int _increase;

    public UseBuffCard(BuffType buffType, CharacterBase target, int increase)
    {
        _buffType = buffType;
        _target = target;
        _increase = increase;
    }

    public async Task Execute()
    {
        switch (_buffType)
        {
            case BuffType.Power:
                _target.IncreaseAttackPower(_increase);
                break;
            case BuffType.Cost:
                if(_target is ICardUser user)
                {
                    user.Cost.ModifyMaxCost(_increase);
                }
                break;
        }
        await Task.CompletedTask;
    }

    public void Undo()
    {
        switch (_buffType)
        {
            case BuffType.Power:
                _target.DecreaseAttackPower(_increase);
                break;
        }
    }
}
