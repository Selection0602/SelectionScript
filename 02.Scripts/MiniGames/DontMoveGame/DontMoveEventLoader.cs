using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DontMoveEventLoader : MonoBehaviour
{
    [Header("이벤트 라벨")]
    [SerializeField] private string label = "DontMoveEvent";

    //이벤트 딕셔너리
    public Dictionary<int, DontMoveEvent> EventDict = new Dictionary<int, DontMoveEvent>();
    public event Action OnLoaded;

    public void LoadEvents()
    {
        Addressables.LoadAssetsAsync<GameObject>(label, null).Completed += OnEventsLoaded;
    }

    private void OnEventsLoaded(AsyncOperationHandle<IList<GameObject>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (GameObject asset in handle.Result)
            {
                DontMoveEvent evt = asset.GetComponent<DontMoveEvent>();
                if (evt != null && !EventDict.ContainsKey(evt.EventNumber))
                {
                    EventDict.Add(evt.EventNumber, evt);
                }
            }

            OnLoaded?.Invoke();
        }
    }

    public DontMoveEvent GetEvent(int number)
    {
        if (EventDict.ContainsKey(number)) return EventDict[number];
        else return null;
    }

    public int GetEventCount()
    {
        return EventDict.Count;
    }
}
