using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EnemyTurnStart : ICommand
{
    private List<CharacterBase> _enemies;
    private ICardDistributor _distributor;
    private bool _isFirstTurn;

    public EnemyTurnStart(ICardDistributor distributor,List<CharacterBase> enemies, bool isFirstTurn)
    {
        _enemies = enemies;
        _distributor = distributor;
        _isFirstTurn = isFirstTurn;
    }

    public async Task Execute()
    {
        if (!_isFirstTurn)
        {
            Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_GetCost);
        }
        foreach (var enemy in _enemies)
        {
            enemy.OnTurnStart();
            if (enemy is IUniqueSkillUser skillUser)
            {
                skillUser.IsCanUseSkill = true;
            }
            if (enemy is Enemy user)
            {
                user.RecoveryCost();
                _distributor.DistributeCards(user);
            }
        }
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
