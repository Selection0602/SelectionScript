using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Buff", menuName = "GameData/CreateBuffData")]

public class BuffSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string BuffName;
	public BuffType BuffType;
	public string Desc;
	public int Damage;
	public int TurnDuration;

}
