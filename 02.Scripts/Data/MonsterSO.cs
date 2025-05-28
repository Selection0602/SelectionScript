using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Monster", menuName = "GameData/CreateMonsterData")]

public class MonsterSO : ScriptableObject
{
	public string FileName;
	public UnityEngine.Sprite Image;
	public int Index;
	public string Name;
	public MonsterType MonsterType;
	public int Health;
	public int Power;
	public int Cost;
	public int CardIndex;
	public int UniqueSkillIndex;

}
