using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Mini Game/Avoid Monster/Item Data")]
public class AvoidItemData : ScriptableObject
{
    public string ItemName;                // 아이템 이름
    public Sprite ItemSprite;              // 아이템 이미지
    public string[] DialogueTexts;         // 대화 텍스트 배열
    [TextArea(3, 5)]
    public string Description;             // 아이템 설명

    public int ItemID; // 아이템 ID
    public bool canSpawn; // 아이템 스폰 가능 여부
}
