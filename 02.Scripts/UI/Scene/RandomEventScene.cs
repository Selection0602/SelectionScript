using System.Collections.Generic;
using UnityEngine;

public class RandomEventScene : SceneBase
{
    [SerializeField] private RandomEventType _randomEventType;
    public RandomEventController EventController { get; private set; }

    protected override void OnStart(object data)
    {
        base.OnStart(data);

        // 맵 씬에서 받아온 데이터 적용
        if (data is IEnumerable<object> list)
        {
            foreach (var obj in list)
            {
                if (obj is RandomEventType eventType)
                {
                    _randomEventType = eventType;
                }
            }
        }
        
        EventController = GetComponent<RandomEventController>();

        if (EventController)
            EventController.StartEvent(_randomEventType);
    }

    public void MoveMapScene()
    {
        var labelMapping = new AssetLabelMapping[]
        {
            new AssetLabelMapping
            {
                label = "NodeType",
                assetType = AssetType._NodeTypeDataSO
            },
            new AssetLabelMapping
            {
                label = "Character",
                assetType = AssetType.CharacterSO
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
        var loadData = new LoadingSceneData
        {
            mappings = labelMapping,
            tipChangeInterval = 2f,
            nextSceneName = "MapScene",
            payload = new object[] { }
        };

        foreach (var mappings in labelMapping)
        {
            if (mappings.label is "BGM" or "SFX")
            {
                mappings.label += $", {loadData.nextSceneName}";
            }
        }
        LoadScene("LoadingScene", loadData);
    }
}