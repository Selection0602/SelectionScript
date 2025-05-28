using UnityEngine;

public class BossFactory : IEnemyFactory
{
    private readonly BattleDataManager _battleDataManager;

    public BossFactory(BattleDataManager battleDataManager)
    {
        _battleDataManager = battleDataManager;
    }

    public CharacterBase CreateEnemy(GameObject gameObject, MonsterSO data, EnemyDeckHandler deckHandler)
    {
        var battleCharacterData = CreateBattleBossData(data);

        switch (data.FileName)
        {
            case "Ophanim":
                var ophanim = gameObject.AddComponent<Ophanim>();
                ophanim.Initialize(battleCharacterData);
                ophanim.SetDeckHandler(deckHandler);
                return ophanim;
            case "Demon":
                var demon = gameObject.AddComponent<Demon>();
                demon.Initialize(battleCharacterData);
                demon.SetDeckHandler(deckHandler);
                return demon;
            default:
                return null;
        }
    }

    private BattleCharacterData CreateBattleBossData(MonsterSO data)
    {
        return new BattleCharacterData
        {
            Sprite = data.Image,
            MaxSantity = data.Health,
            CurrentSantity = data.Health,
            BaseAttackPower = data.Power,
            CardIndex = data.CardIndex,
            Cost = data.Cost,
            SkillData = _battleDataManager.SkillDatas[data.UniqueSkillIndex]
        };
    }
}