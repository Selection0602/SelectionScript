using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class EndingCombiner : MonoBehaviour
{
    private AddressableManager _addressableManager;
    private Dictionary<int, MemorySO> _playerMemories = new Dictionary<int, MemorySO>();
    
    private List<EndingSO> _endingList;
    private EndingSO _basicEnding;

    [FormerlySerializedAs("isReady")] public bool IsReady = false;
    
    private async void Start()
    {
        try
        {
            _addressableManager = Manager.Instance.AddressableManager;
            _playerMemories = Manager.Instance.DataManager.Memories;
        
            await SetEndings();
        }
        catch (Exception e)
        {
            Debug.LogError($"EndingCombiner 초기화 중 오류 발생: {e.Message}");
            IsReady = false;
        }
    }

    private async Task SetEndings()
    {
        _endingList = await _addressableManager.LoadGroupAssetsAsync<EndingSO>("Ending");
        SortEndings();
        IsReady = true;
    }
    
    private void SortEndings()
    {
        if (_endingList == null || _endingList.Count == 0) return;

        // 기본 엔딩(무의미) 찾기
        foreach (var ending in _endingList)
        {
            if (ending.Index == 1)
            {
                _basicEnding = ending;
                break;
            }
        }
        
        // 엔딩 리스트 내림차순 정렬
        int n = _endingList.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (_endingList[j].Index < _endingList[j + 1].Index)
                {
                    (_endingList[j], _endingList[j + 1]) = (_endingList[j + 1], _endingList[j]);
                }
            }
        }
    }
    
    /// <summary>
    /// 플레이어가 가지고 있는 메모리를 기반으로 엔딩 반환
    /// </summary>
    /// <returns>유효한 엔딩 반환</returns>
    public EndingSO GetEndingSO()
    {
        // 플레이어가 가지고 있는 메모리가 없다면 기본 엔딩 반환
        if(_playerMemories.Count < 2)
        {
            // 엔딩을 본 기록 추가
            Manager.Instance.CreditManager.UnlockEndingAndSave(_basicEnding.Index);

            return _basicEnding;
        }
        
        // 현재 캐릭터 인덱스 가져오기
        int characterIndex = Manager.Instance.DataManager.CurrentCharcterId;
        
        foreach (var ending in _endingList)
        {
            // Selection 엔딩 혹은 캐릭터 인덱스가 일치하지 않는다면 패스
            if (ending.Index == 11 || (ending.CharacterIndex != 0 && ending.CharacterIndex != characterIndex)) continue;
            
            // 무의미까지 유효한 엔딩이 없다면 무의미 엔딩 반환
            if (ending.Index == 0)
            {
                // 엔딩을 본 기록 추가
                Manager.Instance.CreditManager.UnlockEndingAndSave(ending.Index);
                return ending;
            }
            
            bool isValid = true;
            
            foreach (var index in ending.EndingTrigger)
            {
                if (!_playerMemories.ContainsKey(index))
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                continue;
            else
            {
                // 엔딩을 본 기록 추가
                Manager.Instance.CreditManager.UnlockEndingAndSave(ending.Index);

                return ending;
            }
        }
        
        // 모두 해당되지 않는다면 무의미 엔딩 반환
        Manager.Instance.CreditManager.UnlockEndingAndSave(_basicEnding.Index);

        return _basicEnding;
    }

    // 본 엔딩만 언락하는 메소드
    public void UnlockViewedEndings()
    {
        if (_endingList == null || _endingList.Count == 0)
            return;
        
        foreach (var ending in _endingList)
        {
            // 이미 본 엔딩만 언락
            ending.IsLocked = !Manager.Instance.DataManager.HasViewedEnding(ending.Index);
        }
    }
}
