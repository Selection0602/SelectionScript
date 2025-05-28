using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UseDebuffCard : ICommand
{
    private IEnumerable<CharacterBase> _targets;
    private DebuffSO _debuffData;

    public UseDebuffCard(IEnumerable<CharacterBase> targets, DebuffSO debuffData)
    {
        _targets = targets;
        _debuffData = debuffData;
    }

    public async Task Execute()
    {
        foreach (var target in _targets)
        {
            await Debuff(target);
        }
    }

    private async Task  Debuff(CharacterBase target)
    {
        var debuff = new Debuff();

        //타겟이 플레이어 이고, 디버프 저항이 있으면 
        if (target is Player player && player.DebuffResistance.Contains(_debuffData.DebuffType))
        {
            Debug.Log($"{_debuffData.DebuffType}디버프 저항");
            return;
        }

        debuff.TurnEffectIcon = _debuffData.Icon;

        switch (_debuffData.DebuffType)
        {
            case DebuffType.Burn:
                debuff.Duration = _debuffData.TurnDuration;
                debuff.OnTurnEnd = async() => { await target.Damage(_debuffData.Damage); };
                break;
            case DebuffType.Poison:
                debuff.IsInfinite = true;
                debuff.OnTurnEnd = async () => { await target.Damage(_debuffData.Damage); };
                break;
            case DebuffType.Weaken:
                debuff.Duration = _debuffData.TurnDuration;
                debuff.OnDurationEnd = () => { target.DecreaseDamageTaken(_debuffData.Damage); };
                target.IncreaseDamageTaken(_debuffData.Damage);
                break;
        }
        target.AddDebuff(debuff);
        await Task.CompletedTask;
    }

    public void Undo()
    {

    }
}
