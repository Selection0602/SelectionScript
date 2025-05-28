using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneController : SceneBase
{
    public BattleManager BattleManager;
    public BattleDataManager BattleDataManager;

    private NodeType _nodeType;

#if UNITY_EDITOR
    public NodeType TestNodeType = NodeType.None;
#endif

    private float _battleTime = 0f;

    protected override async void OnStart(object data)
    {
        base.OnStart(data);

        _nodeType = GetNodeType(data);
        if(_nodeType == NodeType.None)
        {
            _nodeType = NodeType.NormalBattle;
        }

        await BattleDataManager.LoadBattleDatas();
        await BattleDataManager.LoadAnimations();
        await BattleDataManager.LoadBattleUI(_nodeType);

        BattleManager.GameInitialize(_nodeType);
    }

    private void Update()
    {
        _battleTime += Time.unscaledDeltaTime;
    }

    private NodeType GetNodeType(object data)
    {
#if UNITY_EDITOR
        if (TestNodeType != NodeType.None)
        {
            return TestNodeType;
        }
        if (data is IEnumerable<System.Object> list)
        {
            foreach (var obj in list)
            {
                if (obj is NodeType nodeType)
                {
                    return nodeType;
                }
            }
        }
        return NodeType.None;
#else
        if (data is IEnumerable<System.Object> list)
        {
            foreach (var obj in list)
            {
                if (obj is NodeType nodeType)
                {
                    return nodeType;
                }
            }
        }
        return NodeType.None;
#endif
    }

    public void MoveNextScene(bool isClear, MemorySO memory = null)
    {
        Manager.Instance.AnalyticsManager.LogEvent(EventName.BATTLE_COMPLETED_TIME, new Dictionary<string, object>()
        {
            {EventParam.CLEAR_TIME, (int)_battleTime},
            {EventParam.NODE_TYPE, _nodeType.ToString()}
        });

        //클리어 했을때
        if (isClear)
        {
            if (_nodeType == NodeType.BossBattle)
            {
                MoveResultScene(isClear, memory);
            }
            else
            {
                MoveMapScene();
            }
        }
        else
        {
            MoveResultScene(isClear);
        }
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
            payload = new object[] { } // 배열로 감싸줌
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

    public void MoveResultScene(bool isClear, MemorySO memory = null)
    {
        var labelMapping = new AssetLabelMapping[]
        {
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
            nextSceneName = "ResultScene",
            payload = new object[] { isClear, memory } // 배열로 감싸줌
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

    public void OnBackButtonPressed()
    {
        Manager.Instance.AnalyticsManager.LogEvent(EventName.BACK_BUTTON_CLICKED, EventParam.SCENE_NAME, SceneName);
    }
}
