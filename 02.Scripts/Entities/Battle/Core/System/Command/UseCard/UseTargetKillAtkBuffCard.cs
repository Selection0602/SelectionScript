using System;
using System.Threading.Tasks;

public class UseTargetKillAtkBuffCard : ICommand
{
    private CharacterBase _target;
    private CharacterBase _user;
    private int _damage;
    private int _increaseAttackPower;

    public UseTargetKillAtkBuffCard(CharacterBase target, CharacterBase user, int damage, int increaseAttackPower)
    {
        _target = target;
        _user = user;
        _damage = damage;
        _increaseAttackPower = increaseAttackPower;
    }

    public async Task Execute()
    {
        if(_user is Enemy enemy)
        {
            await enemy.PlayAttackAnim();
        }
        await _target.Damage(_damage);
        if (_target.WillDieFromDamage(_damage) && _user != null)
        {
            _user.IncreaseAttackPower(_increaseAttackPower);
        }
    }

    public void Undo()
    {

    }
}
