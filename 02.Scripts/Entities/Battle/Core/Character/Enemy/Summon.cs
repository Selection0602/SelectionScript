using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Summon : CharacterBase
{
    private EnemyAnimController _animController;
    private ISummoner _summoner;

    public override void Initialize(BattleCharacterData data)
    {
        _animController = GetComponentInChildren<EnemyAnimController>();
        _characterImage = _animController.GetComponent<Image>();

        base.Initialize(data);
        var monsterZone = GetComponentInChildren<MonsterDropZone>();
        _effectPos = monsterZone.transform;
        monsterZone.Initialize(this);
    }

    public void SetSummoner(ISummoner summoner )
    {
        _summoner = summoner;
    }

    public override async Task Dead()
    {
        _summoner.DeathSummon(this);
        _attackPowerText.gameObject.SetActive(false);
        await _animController.PlayDeathAnim();
        _manager.OnEnemyDied(this);
        gameObject.SetActive(false);
    }
}
