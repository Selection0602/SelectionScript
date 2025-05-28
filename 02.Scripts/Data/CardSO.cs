using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Card", menuName = "GameData/CreateCardData")]

public class CardSO : ScriptableObject
{
	public string FileName;
	public UnityEngine.Sprite Image;
	public int Index;
	public CardRarityType RarityType;
	public TargetType TargetType;
	public EffectRange EffectRange;
	public string CardName;
	public int Cost;
	public string Desc;
	public string Values;
	public System.Collections.Generic.List<int> ApplyValues;
	public bool IsDispaly;
	public bool ReDraw;
	public int Weight;

}
