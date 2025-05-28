using UnityEngine;
using UnityEditor;
[System.Serializable]
public class JsonToSO : MonoBehaviour
{
	[MenuItem("Tools/JsonToSO/CreateCharacterSO")]
	static void CharacterDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<CharacterData>("Character.json", typeof(CharacterSO));
	}
	[MenuItem("Tools/JsonToSO/CreateMonsterSO")]
	static void MonsterDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<MonsterData>("Monster.json", typeof(MonsterSO));
	}
	[MenuItem("Tools/JsonToSO/CreateSkillSO")]
	static void SkillDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<SkillData>("Skill.json", typeof(SkillSO));
	}
	[MenuItem("Tools/JsonToSO/CreateCardSO")]
	static void CardDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<CardData>("Card.json", typeof(CardSO));
	}
	[MenuItem("Tools/JsonToSO/CreateValueSO")]
	static void ValueDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<ValueData>("Value.json", typeof(ValueSO));
	}
	[MenuItem("Tools/JsonToSO/CreateDebuffSO")]
	static void DebuffDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<DebuffData>("Debuff.json", typeof(DebuffSO));
	}
	[MenuItem("Tools/JsonToSO/CreateMemorySO")]
	static void MemoryDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<MemoryData>("Memory.json", typeof(MemorySO));
	}
	[MenuItem("Tools/JsonToSO/CreateEndingSO")]
	static void EndingDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<EndingData>("Ending.json", typeof(EndingSO));
	}
	[MenuItem("Tools/JsonToSO/CreateTipSO")]
	static void TipDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<TipData>("Tip.json", typeof(TipSO));
	}
	[MenuItem("Tools/JsonToSO/CreateStartSO")]
	static void StartDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<StartData>("Start.json", typeof(StartSO));
	}
	[MenuItem("Tools/JsonToSO/CreateRewardSO")]
	static void RewardDataInit()
	{
		DynamicMenuCreator.CreateMenusFromJson<RewardData>("Reward.json", typeof(RewardSO));
	}

}
