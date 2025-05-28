using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Reward", menuName = "GameData/CreateRewardData")]

public class RewardSO : ScriptableObject
{
	public string FileName;
	public UnityEngine.Sprite Image;
	public UnityEngine.Sprite Icon;
	public int Index;
	public string RewardName;
	public string Desc;
	public int AtkUpValue;
	public int HealUpValue;
	public int MaxHealValue;
	public int DrawCard;
	public int CostDown;
	public int NotBurn;
	public int NotPoison;
	public int NotWeaken;
	public int GetCard;
	public int GetCardIndex;
	public int CardDrawIndex;
	public RewardType TypeValue;

}
