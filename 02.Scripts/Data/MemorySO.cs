using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Memory", menuName = "GameData/CreateMemoryData")]

public class MemorySO : ScriptableObject
{
	public string FileName;
	public UnityEngine.Sprite Image;
	public int Index;
	public string MemoryName;
	public string Desc;
	public int AtkValue;
	public int HealValue;
	public int Stack;
	public bool IsBoss;

}
