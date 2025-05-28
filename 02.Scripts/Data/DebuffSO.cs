using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Debuff", menuName = "GameData/CreateDebuffData")]

public class DebuffSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string BuffName;
	public DebuffType DebuffType;
	public string Desc;
	public int Damage;
	public int TurnDuration;
	public UnityEngine.Sprite Icon;

}
