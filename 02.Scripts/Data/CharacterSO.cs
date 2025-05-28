using UnityEngine;
using System.Collections.Generic;
using Live2D.Cubism.Core;

[CreateAssetMenu(fileName = "Character", menuName = "GameData/CreateCharacterData")]

public class CharacterSO : ScriptableObject
{
	public string FileName;
	public Sprite Image;
	public int Index;
	public string Name;
	public string Desc;
	public int UniqueSkillIndex;
	public int Health;
	public int MaximumCost;
	public int Power;
	public int CardIndex;
	public CubismModel Prefab;
}
