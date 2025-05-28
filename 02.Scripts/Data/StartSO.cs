using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "Start", menuName = "GameData/CreateStartData")]

public class StartSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string Desc;

}
