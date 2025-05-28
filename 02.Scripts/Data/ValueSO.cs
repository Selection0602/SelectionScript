using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Value", menuName = "GameData/CreateValueData")]

public class ValueSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string DisplayName;
	public CardEffectType EffectType;

}
