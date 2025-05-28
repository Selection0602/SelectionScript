using System.Collections.Generic;
using UnityEngine;

public class MiniGameResult : SceneBase
{
    private bool fromStartScene = false;
    private float _time = 0;

    protected override void OnStart(object data)
    {
        base.OnStart(data);
        
        // payload에서 StartScene에서 왔는지 확인
        if (scenePayload is object[] payloadArray && payloadArray.Length > 2)
        {
            if (payloadArray[2] is bool fromStart)
            {
                fromStartScene = fromStart;
                
                // StartScene에서 진입했을 경우 체력 UI 비활성화
                if (fromStartScene)
                {
                    var upperBar = GameObject.Find("UpperBar");
                    if (upperBar != null)
                    {
                        upperBar.SetActive(false);
                    }
                }
            }
        }
    }

    private void Update()
    {
        _time += Time.unscaledDeltaTime;
    }

    public void MiniGameManage(bool isSuccess)
    {
        // payload에서 bool 값을 확인
        bool returnToStart = false;
        
        // 현재 씬의 payload 가져오기
        var currentScene = Current;
        if (currentScene != null)
        {
            Debug.Log($"현재 씬: {SceneName}, Current: {currentScene.GetType().Name}");
            
            // GetPayload 메서드로 스크립트에서 직접 참조 없이 payload 접근
            var payload = currentScene.GetPayload<object>();
            Debug.Log("페이로드: " + (payload != null ? payload.ToString() : "null"));
            
            // payload가 배열 형태로 전달되었을 경우
            if (payload is object[] payloadArray)
            {
                Debug.Log("Payload 배열 길이: " + payloadArray.Length);
                for (int i = 0; i < payloadArray.Length; i++)
                {
                    Debug.Log($"Payload[{i}] = {payloadArray[i]}");
                }
                
                if (payloadArray.Length > 1 && payloadArray[1] is bool returnFlag)
                {
                    returnToStart = returnFlag;
                    Debug.Log("returnToStart 값: " + returnToStart);
                }
            }
        }

        // 시작 씬에서 왔는지 확인 (크레딧 용도)
        if (fromStartScene)
        {
            // 시작 씬에서 왔을 경우 체력 관련 로직 없이 씬 이동만 처리
            GoToNextScene(returnToStart);
        }
        else
        {
            // 일반 게임 플레이 상황
            if (isSuccess)
            {
                // 성공 시 정신력 회복
                Manager.Instance.DataManager.Heal(30);
            }
            else
            {
                // 실패 시 정신력 감소
                Manager.Instance.DataManager.Damage(20);
            }

            // 다음 씬으로 이동
            GoToNextScene(returnToStart);
        }

        Manager.Instance.AnalyticsManager.LogEvent(EventName.MINIGAME_PLAYED, new Dictionary<string, object>()
        {
            {EventParam.MINIGAME_NAME, SceneName},
            {EventParam.CLEAR_TIME, _time}
        });
    }
    private void GoToNextScene(bool returnToStart)
    {
        if (returnToStart)
        {
            // StartScene으로 돌아가는 경우
            var labelMapping = new AssetLabelMapping[]
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
        else
        {
            // 기존처럼 MapScene으로 이동하는 경우
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

            var loadData = CreateLoadingSceneData(labelMapping, "MapScene", new object[] { });
            LoadScene("LoadingScene", loadData);
        }
    }

    public void OnBackButtonPressed()
    {
        Manager.Instance.AnalyticsManager.LogEvent(EventName.BACK_BUTTON_CLICKED, EventParam.SCENE_NAME, SceneName);
    }
}
