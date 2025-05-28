using System.Collections.Generic;
using System.Threading.Tasks;

public class UseAttackCard : ICommand
{
    private ICardUser _user;
    private List<CharacterBase> _targets;
    private int _damage;
    private bool _isPlayAttackAnim = true;

    public UseAttackCard(ICardUser user, List<CharacterBase> targets, int damage, bool isPlayAttackAnim = true)
    {
        _user = user;
        _targets = targets;
        _damage = damage;
        _isPlayAttackAnim = isPlayAttackAnim;
    }

    public async Task Execute()
    {
        foreach (var target in _targets)
        {
            if (_user.Character is Enemy enemy && _isPlayAttackAnim)
            {
                await enemy.PlayAttackAnim();
            }
            await target.Damage(_damage);
        }
    }

    public void Undo()
    {
        foreach (var target in _targets)
        {
            target.Heal(_damage);
        }
    }
}
