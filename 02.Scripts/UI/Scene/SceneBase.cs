using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBase : MonoBehaviour
{
    public static SceneBase Current { get; private set; } = null;

    private static object sceneBridgeData; //씬 전환시 건네줄 데이터

    public static string SceneName; //씬 이름
    protected object scenePayload;
    public T GetPayload<T>() => (T)scenePayload;

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        OnStart(sceneBridgeData);
    }

    protected virtual void OnStart(object data)
    {
        scenePayload = data;
        SceneName = SceneManager.GetActiveScene().name;
    }

    //씬 전환
    public static void LoadScene(string sceneName, object data = null)
    {
        sceneBridgeData = data;
        SceneManager.LoadSceneAsync(sceneName);
    }

    public static T GetCurrent<T>() where T : SceneBase
    {
        return Current as T;
    }
    
    // 로딩 씬 데이터 생성을 위한 유틸리티 메서드
    protected static LoadingSceneData CreateLoadingSceneData(AssetLabelMapping[] mappings, string nextSceneName, object payload, float tipInterval = 2f)
    {
        var loadData = new LoadingSceneData
        {
            mappings = mappings,
            tipChangeInterval = tipInterval,
            nextSceneName = nextSceneName,
            payload = payload
        };

        foreach (var mapping in mappings)
        {
            if (mapping.label is "BGM" or "SFX")
            {
                mapping.label += $", {nextSceneName}";
            }
        }
        
        return loadData;
    }
}