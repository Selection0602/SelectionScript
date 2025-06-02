using System.Collections.Generic;
using UnityEngine;

public class CreditManager
{
    private Dictionary<int, EndingSO> _endingDatas = new Dictionary<int, EndingSO>();
    public IReadOnlyDictionary<int, EndingSO> EndingDatas => _endingDatas;
    private Dictionary<int, MiniGameDataSO> _miniGameDatas = new Dictionary<int, MiniGameDataSO>();
    private HashSet<int> _unlockedEndings = new HashSet<int>();
    private HashSet<int> _unlockedMiniGames = new HashSet<int>();

    #region ------------------------- 엔딩 ------------------------- 
    public void InitializeEndings(List<EndingSO> endings)
    {
        _endingDatas.Clear();
        foreach (var ending in endings)
        {
            _endingDatas[ending.Index] = ending;
        }
    }
    public bool IsEndingLocked(int endingIndex)
    {
        bool isLocked = !_unlockedEndings.Contains(endingIndex);
        return isLocked;
    }

    public void UnlockEnding(int endingIndex)
    {
        if (_endingDatas.ContainsKey(endingIndex))
        {
            _unlockedEndings.Add(endingIndex);
        }
    }
    public void UnlockEndingAndSave(int endingIndex)
    {
        // 이미 해금된 엔딩인지 확인
        if (_unlockedEndings.Contains(endingIndex))
        {
            return;
        }
        // 엔딩 해금
        UnlockEnding(endingIndex);
        
        // DataManager에 추가
        Manager.Instance.DataManager.AddViewedEnding(endingIndex);
        
        // 영구 데이터 저장
        Manager.Instance.SaveManager.SaveCredits();
    }
    #endregion

    #region ------------------------- 미니게임 -------------------------
    public void InitializeMiniGames(List<MiniGameDataSO> miniGames)
    {
        _miniGameDatas.Clear();
        foreach (var miniGame in miniGames)
        {
            _miniGameDatas[miniGame.index] = miniGame;
        }
    }
    public void UnlockMiniGameAndSave(int miniGameIndex)
    {
        // 이미 해금된 미니게임인지 확인
        if (_unlockedMiniGames.Contains(miniGameIndex))
        {
            return;
        }
        // 미니게임 해금
        UnlockMiniGame(miniGameIndex);

        // DataManager에 추가
        Manager.Instance.DataManager.AddUnlockedMiniGame(miniGameIndex);

        // 영구 데이터 저장
        Manager.Instance.SaveManager.SaveCredits();
    }
    public bool IsMiniGameLocked(int miniGameIndex)
    {
        bool isLocked = !_unlockedMiniGames.Contains(miniGameIndex);
        return isLocked;
    }
    public void UnlockMiniGame(int miniGameIndex)
    {
        if (_miniGameDatas.ContainsKey(miniGameIndex))
        {
            _unlockedMiniGames.Add(miniGameIndex);
        }
    }
    #endregion
}