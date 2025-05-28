using UnityEngine;

[CreateAssetMenu(fileName = "TypeImage", menuName = "GameData/CreateTypeImageData")]
public class CharacterTypeImageSO : ScriptableObject
{
    public Sprite CostImage;
    public Sprite DeckImage;
    public Sprite CardBackImage;
    public Sprite CardFrontImage;
    public Sprite SkillIcon;
    public Color CostLightColor;
}
