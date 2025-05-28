using System.Collections.Generic;
using UnityEngine;

public enum TutorialType
{
    Main,
    Memory,
    BossSkill,
}


[System.Serializable]
public class TutorialTargetInfo
{
    public string TargetId;
    public Vector2 UIOffset;
}

[CreateAssetMenu(fileName = "New Tutorial Data", menuName = "Tutorial/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [Header("텍스트 내용")]
    public string TitleText;
    [TextArea(3, 5)] public string DescText;
    
    public List<TutorialTargetInfo> TargetInfos;
    public TutorialType Type = TutorialType.Main;

}