using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Tip", menuName = "GameData/CreateTipData")]

public class TipSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string Desc;

}
