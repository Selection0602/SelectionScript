using UnityEngine;

public interface ICharacterFactory
{
    public Player CreateCharacter(GameObject gameObject,CharacterSO data);
}

public class CharacterFactory : ICharacterFactory
{
    private BattleDataManager _battleDataManager;

    public CharacterFactory(BattleDataManager  battleDataManager)
    {
        _battleDataManager = battleDataManager;
    }

    public Player CreateCharacter(GameObject gameObject, CharacterSO data)
    {
        var battleData = CreateBattleCharacterData(data);

        switch (data.FileName)
        {
            case "Maria":
                var maria = gameObject.AddComponent<Maria>();
                maria.Initialize(battleData);
                return maria;
            case "Elios":
                var elios = gameObject.AddComponent<Elios>();
                elios.Initialize(battleData);
                return elios;
            default:
                return null;
        }
    }

    private BattleCharacterData CreateBattleCharacterData(CharacterSO playerData)
    {
        return new BattleCharacterData
        {
            Sprite = playerData.Image,
            MaxSantity = Manager.Instance.DataManager.MaxSanity,
            BaseAttackPower = playerData.Power,
            BounsAttackPower = Manager.Instance.DataManager.BounsAttackPower,
            CurrentSantity = Manager.Instance.DataManager.CurrentSanity,
            CardIndex = playerData.CardIndex,
            Cost = playerData.MaximumCost,
            SkillData = _battleDataManager.SkillDatas[playerData.UniqueSkillIndex]
        };
    }
} 