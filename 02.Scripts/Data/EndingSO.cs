using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ending", menuName = "GameData/CreateEndingData")]
public class EndingSO : ScriptableObject
{
    public string FileName;
    public int Index;
    public string EndingName;
    public int CharacterIndex;
    public List<int> EndingTrigger;
    public string EndingTriggerString;
    public string EndingText;
    public Sprite EndingImage;
    
    [SerializeField]
    private bool _isLocked = true;
    
    public bool IsLocked
    {
        get => _isLocked;
        set => _isLocked = value;
    }
}
