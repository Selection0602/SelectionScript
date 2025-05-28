using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum RandomEventType
{
    Earring,
    Cake,
    RandomMemory,
    RandomBooty,
    LoseBooty
}

public class MapScene : SceneBase
{
    [SerializeField] private NodeSceneMapping _nodeSceneMapping;
    [SerializeField] private Image _fadeImage;

    override protected void OnStart(object data)
    {
        base.OnStart(data);
        FadeIn();
    }

    /// <summary>
    /// 노드 클릭 시 실행 될 이벤트 함수
    /// </summary>
    /// <param name="nodeType">다음 씬에 넘겨줄 노드 타입</param>
    public void LoadNodeTypeScene(NodeType nodeType) 
    {
        FadeOut(() =>
        {
            LoadGameScene(nodeType);
        });
    }
    
    private void LoadGameScene(NodeType nodeType)
    {
        DOTween.KillAll();

        NodeSceneMapping.NodeTypeMapping mapping;
        object[] payload;

        if(nodeType != NodeType.RandomEvent)
        {
            mapping = _nodeSceneMapping.GetNodeTypeMapping(nodeType);
            payload = new object[] { nodeType };
        }
        else
        {
            var (eventMapping, eventType) = _nodeSceneMapping.GetRandomEventMapping();
            mapping = eventMapping;
            payload = new object[] { eventType };
        }
    
        var loadData = new LoadingSceneData
        {
            mappings = mapping.LabelMappings,
            tipChangeInterval = mapping.LoadingTipInterval,
            nextSceneName = mapping.SceneName,
            payload = payload
        };

        LoadScene("LoadingScene", loadData);
    }

    public void ReturnStartScene()
    {
        var labelMapping = new[]
        {
            new AssetLabelMapping
            {
                label = "Ending",
                assetType = AssetType.EndingSO
            },
            new AssetLabelMapping
            {
                label = "MiniGame",
                assetType = AssetType.MiniGameDataSO
            },
            new AssetLabelMapping
            {
                label = "BGM",
                assetType = AssetType.AudioClip
            },
            new AssetLabelMapping
            {
                label = "SFX",
                assetType = AssetType.AudioClip
            }
        };
        
        var loadData = CreateLoadingSceneData(labelMapping, "StartScene", null);
        
        LoadScene("LoadingScene", loadData);
    }

    public void FadeIn(Action onComplete = null)
    {
        _fadeImage.color = new Color(0, 0, 0, 1);
        _fadeImage.gameObject.SetActive(true);

        _fadeImage.DOFade(0, 1f).SetDelay(0.3f).OnComplete(() => 
        {
            onComplete?.Invoke();

                _fadeImage.gameObject.SetActive(false);
        });
    }

    public void FadeOut(Action onComplete = null)
    {
        _fadeImage.color = new Color(0, 0, 0, 0);
        _fadeImage.gameObject.SetActive(true);

        _fadeImage.DOFade(1, 0.5f).OnComplete(() => 
        {
            onComplete?.Invoke();
        });
    }
}
