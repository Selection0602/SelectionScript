using System.Collections.Generic;

[System.Serializable]
public class ItemRecipe
{
    public List<int> IngredientIdList = new List<int>(); // 재료 아이템 ID 리스트
    public int ResultId; // 결과 아이템 ID
}
