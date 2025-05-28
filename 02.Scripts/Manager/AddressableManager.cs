using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager
{
    private Dictionary<string, AsyncOperationHandle> handles = new();
    private Dictionary<List<string>, AsyncOperationHandle> listHandles = new();

    public async Task<T> Load<T>(string key, Action<T> onComplete = null)
    {
        var handle = Addressables.LoadAssetAsync<T>(key);
        try
        {
            T data = await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(data);
                return data;
            }
            return default;
        }
        catch
        {
            return default;
        }
    }

    public async Task<T> LoadRandomAsset<T>(string label) where T : UnityEngine.Object
    {
        var assets = await LoadGroupAssetsAsync<T>(label);
        if (assets == null)
        {
            return default;
        }
        var randomAsset = assets[UnityEngine.Random.Range(0, assets.Count)];
        return randomAsset;
    }

    public async Task<List<TObject>> LoadGroupAssetsAsync<TObject>(string label, Action onComplete = null)
    {
        List<TObject> loadedAssests = new();

        var handle = Addressables.LoadAssetsAsync<TObject>(label, asset =>
        {
            loadedAssests.Add(asset);
        });

        await handle.Task;

        if (!handles.ContainsKey(label) && label != "Card" && label != "Reward" && label != "MemoryData")
        {
            handles.Add(label, handle);
        }

        onComplete?.Invoke();

        return loadedAssests;
    }

    public async Task<List<T>> LoadGroupAssetsAsync<T>(List<string> labels, Addressables.MergeMode mergeMode = Addressables.MergeMode.Union, Action onComplete = null) where T : UnityEngine.Object
    {
        List<T> loadedAssets = new();

        var listHandle = Addressables.LoadAssetsAsync<T>
        (
            labels,
            asset =>
            {
                loadedAssets.Add(asset);
            },
            mergeMode,
            false);

        if (!listHandles.ContainsKey(labels))
        {
            listHandles.Add(labels, listHandle);
        }

        await listHandle.Task;
        onComplete?.Invoke();

        return loadedAssets;
    }

    public void UnloadAssets(string label)
    {
        if(!handles.TryGetValue(label, out var handle)) return;
        
        Addressables.Release(handle);
    }

    public void UnloadAssets(List<string> labels)
    {
        if (!listHandles.TryGetValue(labels, out var handle)) return;
        
        Addressables.Release(handle);
        listHandles.Remove(labels);
    }

    public void AllUnloadAssets()
    {
        foreach (var handle in handles.Values)
            Addressables.Release(handle);

        foreach (var handle in listHandles.Values)
            Addressables.Release(handle);

        handles.Clear();
        listHandles.Clear();
    }

    public AsyncOperationHandle GetHandle(string label)
    {
        return handles.GetValueOrDefault(label);
    }
    public async Task<List<TObject>> GetHandleResultList<TObject>(string label)
    {
        var list = new List<TObject>();

        if (handles.TryGetValue(label, out var rawHandle))
        {
            if (rawHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var rawList = rawHandle.Result as IEnumerable<object>;
                if (rawList==null)
                {
                    Debug.LogWarning($"[Addressables] '{label}'에서 TObject 리스트 변환 실패");
                    return new List<TObject>();
                }
                foreach (var raw in rawList)
                {
                    if (raw is TObject typed)
                    {
                        list.Add(typed);
                    }
                }
            }
        }
        else
        {
            list = await LoadGroupAssetsAsync<TObject>(label);
        }
        return list;
    }

    public bool HasHandle(string label) => handles.ContainsKey(label);

    public void RegisterHandle(string label, AsyncOperationHandle handle)
    {
        if (!handles.ContainsKey(label))
        {
            handles[label] = handle;
        }
    }
}
