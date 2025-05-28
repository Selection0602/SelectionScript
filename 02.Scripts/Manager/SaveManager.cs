using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.json");
    private string MapSavePath => Path.Combine(Application.persistentDataPath, "mapsave.json");
    private string PermanentSavePath => Path.Combine(Application.persistentDataPath, "permanentsave.json");

    // RewardSO를 RewardData로 변환
    private RewardData ConvertSOToData(RewardSO so)
    {
        RewardData data = new RewardData();

        // 모든 속성 복사
        data.FileName = so.FileName;
        data.Image = so.Image;
        data.Icon = so.Icon;
        data.Index = so.Index;
        data.RewardName = so.RewardName;
        data.Desc = so.Desc;
        data.AtkUpValue = so.AtkUpValue;
        data.HealUpValue = so.HealUpValue;
        data.MaxHealValue = so.MaxHealValue;
        data.DrawCard = so.DrawCard;
        data.CostDown = so.CostDown;
        data.NotBurn = so.NotBurn;
        data.NotPoison = so.NotPoison;
        data.NotWeaken = so.NotWeaken;
        data.TypeValue = so.TypeValue;
        data.GetCardIndex = so.GetCardIndex;
        data.GetCard = so.GetCard;

        return data;
    }   
    #region ----------------------------- Save -----------------------------
    public void SaveCredits()
    {
        var dataManager = Manager.Instance.DataManager;
        var permanentData = new PermanentData
        {
            unlockedEndings = new List<int>(dataManager.ViewedEndings),
            unlockedMiniGames = new List<int>(dataManager.UnlockedMiniGames)
        };

        string jsonData = JsonUtility.ToJson(permanentData, true);
        File.WriteAllText(PermanentSavePath, jsonData);
    }

    public void SaveMapData()
    {
        var mapManager = Manager.Instance.MapManager;
        if (mapManager.SavedMapData != null)
        {
            string jsonData = MapDataSerializer.ToJson(mapManager.SavedMapData);
            if (!string.IsNullOrEmpty(jsonData))
            {
                File.WriteAllText(MapSavePath, jsonData);
            }
        }
    }
    public void SaveGame()
    {
        var dataManager = Manager.Instance.DataManager;
        var mapManager = Manager.Instance.MapManager;

        SaveData saveData = new SaveData
        {
            currentCharacterId = dataManager.CurrentCharcterId,
            bonusAttackPower = dataManager.BounsAttackPower,
            currentSanity = dataManager.CurrentSanity,
            maxSanity = dataManager.MaxSanity,
        };

        // 맵 데이터 저장 (별도 파일)
        if (mapManager.SavedMapData != null)
        {
            SaveMapData();
        }

        // 덱 저장
        foreach (var card in dataManager.PlayerDeck)
        {
            saveData.playerDeckFileNames.Add(card.Key);
        }

        // 전리품 저장
        foreach (var booty in dataManager.Booties)
        {
            saveData.bootyIndices.Add(booty.Index);
        }

        // 메모리 저장
        foreach (var memory in dataManager.Memories)
        {
            saveData.memoryIndices.Add(memory.Key);
        }

        // 크레딧 데이터 저장 (별도 파일)
        SaveCredits();

        // JSON으로 변환하여 파일에 저장
        string jsonData = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, jsonData);
    }
    #endregion
    #region ----------------------------- Load -----------------------------
    public MapData LoadMapData()
    {
        if (File.Exists(MapSavePath))
        {
            string jsonData = File.ReadAllText(MapSavePath);
            MapData mapData = MapDataSerializer.FromJson(jsonData);

            return mapData;
        }
        return null;
    }

    public async Task LoadGame()
    {
        string jsonData = File.ReadAllText(SavePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

        // DataManager 가져오기
        var dataManager = Manager.Instance.DataManager;
        var addressableManager = Manager.Instance.AddressableManager;
        var mapManager = Manager.Instance.MapManager;

        // 데이터 초기화
        dataManager.ClearCharacterData();

        // 캐릭터 정보 불러오기
        CharacterSO characterData = await addressableManager.Load<CharacterSO>(saveData.currentCharacterId.ToString());
        if (characterData != null)
        {
            dataManager.PlayerDeck.Clear();
            await dataManager.InitializeCharacter(characterData);
        }

        // BounsAttackPower 설정
        dataManager.BounsAttackPower = saveData.bonusAttackPower;

        // 카드 덱 복원
        LoadPlayerDeck(saveData.playerDeckFileNames, dataManager);

        // 전리품, 메모리 복원
        await LoadRewards(saveData.bootyIndices, addressableManager, dataManager);
        await LoadMemories(saveData.memoryIndices, addressableManager, dataManager);

        // 정신력 설정
        if (saveData.currentSanity > 0)
        {
            dataManager.Heal(saveData.currentSanity - dataManager.CurrentSanity);
        }

        if (saveData.maxSanity > 0 && saveData.maxSanity != dataManager.MaxSanity)
        {
            int difference = saveData.maxSanity - dataManager.MaxSanity;
            if (difference > 0)
            {
                dataManager.IncreaseMaxSanity(difference);
            }
        }

        // 맵 데이터 로드 (별도 파일에서)
        mapManager.SavedMapData = LoadMapData();

        // 크레딧 데이터 로드 (별도 파일에서)
        await LoadCredits();
    }


    private void LoadPlayerDeck(List<string> fileNames, DataManager dataManager)
    {
        foreach (var fileName in fileNames)
        {
            if (dataManager.CardDatas.TryGetValue(fileName, out CardData cardData))
            {
                if (!dataManager.PlayerDeck.ContainsKey(fileName))
                {
                    dataManager.PlayerDeck.Add(fileName, cardData);
                }
                else
                {
                    dataManager.PlayerDeck[fileName] = cardData;
                }
            }
        }
    }

    private async Task LoadRewards(List<int> indices, AddressableManager addressableManager, DataManager dataManager)
    {
        var rewards = await addressableManager.GetHandleResultList<RewardSO>("Reward");
        foreach (var index in indices)
        {
            RewardSO rewardSO = rewards.Find(r => r.Index == index);
            if (rewardSO != null)
            {
                RewardData rewardData = ConvertSOToData(rewardSO);
                if (!dataManager.IsExistBooty(rewardData))
                {
                    dataManager.AddBooty(rewardData);
                }
            }
        }
    }

    private async Task LoadMemories(List<int> indices, AddressableManager addressableManager, DataManager dataManager)
    {
        var memories = await addressableManager.GetHandleResultList<MemorySO>("MemoryData");
        foreach (var index in indices)
        {
            var memory = memories.Find(m => m.Index == index);
            if (memory != null && !dataManager.Memories.ContainsKey(memory.Index))
            {
                dataManager.AddMemory(memory);
            }
        }
    }

    public async Task LoadCredits()
    {
        if (File.Exists(PermanentSavePath))
        {
            string jsonData = File.ReadAllText(PermanentSavePath);

            var permanentData = JsonUtility.FromJson<PermanentData>(jsonData);
            var dataManager = Manager.Instance.DataManager;
            var addressableManager = Manager.Instance.AddressableManager;

            // 엔딩 데이터 로드
            var endings = await addressableManager.GetHandleResultList<EndingSO>("Ending");
            Manager.Instance.CreditManager.InitializeEndings(endings);

            // 미니게임 데이터 로드
            var miniGames = await addressableManager.GetHandleResultList<MiniGameDataSO>("MiniGame");
            Manager.Instance.CreditManager.InitializeMiniGames(miniGames);

            // 엔딩 해금 상태 적용
            foreach (var endingIndex in permanentData.unlockedEndings)
            {
                dataManager.AddViewedEnding(endingIndex);
                Manager.Instance.CreditManager.UnlockEnding(endingIndex);
            }

            // 미니게임 해금 상태 적용
            foreach (var miniGameIndex in permanentData.unlockedMiniGames)
            {
                dataManager.AddUnlockedMiniGame(miniGameIndex);
                Manager.Instance.CreditManager.UnlockMiniGame(miniGameIndex);
            }
        }
    }
    #endregion
    #region ----------------------------- Check Data -----------------------------
    public bool HasSaveFile()
    {
        return File.Exists(SavePath) || File.Exists(MapSavePath);
    }
    public bool HasPermanentSaveFile()
    {
        return File.Exists(PermanentSavePath);
    }
    #endregion
    #region ----------------------------- Delete Data -----------------------------
    public void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }

    public void ClearAllData()
    {
        // 영구 데이터 백업
        var permanentData = new PermanentData();
        if (File.Exists(PermanentSavePath))
        {
            string jsonData = File.ReadAllText(PermanentSavePath);
            permanentData = JsonUtility.FromJson<PermanentData>(jsonData);
        }

        // 게임 데이터 삭제
        DeleteSaveFile();
        if (File.Exists(MapSavePath))
        {
            File.Delete(MapSavePath);
        }

        var dataManager = Manager.Instance.DataManager;
        var mapManager = Manager.Instance.MapManager;

        // 게임 데이터 초기화
        dataManager.ClearCharacterData();
        mapManager.SavedMapData = null;

        // 영구 데이터 복원
        foreach (var endingIndex in permanentData.unlockedEndings)
        {
            dataManager.AddViewedEnding(endingIndex);
        }
        foreach (var miniGameIndex in permanentData.unlockedMiniGames)
        {
            dataManager.AddUnlockedMiniGame(miniGameIndex);
        }

        // 영구 데이터 저장
        SaveCredits();
    }
    #endregion
}

