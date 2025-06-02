using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Ending", menuName = "GameData/CreateEndingData")]

public class EndingSO : ScriptableObject
{
	public string FileName;
	public int Index;
	public string EndingName;
	public int CharacterIndex;
	public System.Collections.Generic.List<int> EndingTrigger;
	public string EndingTriggerString;
	public string EndingText;
	public Sprite EndingImage;
	public bool IsLocked;
}
