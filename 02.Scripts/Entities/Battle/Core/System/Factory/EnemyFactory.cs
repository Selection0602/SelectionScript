using UnityEngine;

public interface IEnemyFactory
{
    public CharacterBase CreateEnemy(GameObject gameObject, MonsterSO data, EnemyDeckHandler deckHandler);
}

public class EnemyFactory : IEnemyFactory
{
    public CharacterBase CreateEnemy(GameObject gameObject, MonsterSO data, EnemyDeckHandler deckHandler)
    {
        var battleCharacterData = CreateBattleEnemyData(data);
        var enemy = gameObject.AddComponent<Enemy>();
        enemy.Initialize(battleCharacterData);
        enemy.SetCostUI();
        enemy.SetDeckHandler(deckHandler);
        return enemy;
    }

    private BattleCharacterData CreateBattleEnemyData(MonsterSO data)
    {
        return new BattleCharacterData
        {
            Sprite = data.Image,
            MaxSantity = data.Health,
            CurrentSantity = data.Health,
            BaseAttackPower = data.Power,
            CardIndex = data.CardIndex,
            Cost = data.Cost,
        };
    }
}