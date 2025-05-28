using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // 캐릭터 정보
    public int currentCharacterId;
    public int bonusAttackPower;
    public int currentSanity;
    public int maxSanity;
    // 카드 덱 정보
    public List<string> playerDeckFileNames = new List<string>();

    // 전리품 정보
    public List<int> bootyIndices = new List<int>();

    // 메모리 정보
    public List<int> memoryIndices = new List<int>();

    public string mapDataJson;

}
[System.Serializable]
public class PermanentData
{
    public List<int> unlockedEndings = new List<int>();
    public List<int> unlockedMiniGames = new List<int>();
}

