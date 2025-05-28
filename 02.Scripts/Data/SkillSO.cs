using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Skill", menuName = "GameData/CreateSkillData")]

public class SkillSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string SkillName;
	public string Desc;
	public int ValueIndex;
	public int PlayCost;

}
