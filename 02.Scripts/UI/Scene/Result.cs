using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Result : SceneBase
{
    public bool isCleared;
    private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private EndingCombiner endingCombiner;
    
    protected override void OnStart(object data)
    {
        base.OnStart(data);
        
        Manager.Instance.MapManager.ClearData();
        
        // payload가 배열일 경우 처리
        if (scenePayload is object[] arr && arr.Length > 0 && arr[0] is bool cleared)
        {
            isCleared = cleared;
        }
        else if (scenePayload is bool cleared2)
        {
            isCleared = cleared2;
        }

        Manager.Instance.AnalyticsManager.LogEvent(EventName.GAME_CLEARED, EventParam.IS_CLEARED, isCleared);

        text = GetComponentInChildren<TextMeshProUGUI>();
        SetResult();
    }

    #region ----------------------------- Load -----------------------------
    private void LoadEnding()
    {
        int endingIndex = endingCombiner.GetEndingSO().Index; // 선택된 엔딩의 인덱스 가져오기
        LoadEndingScene(endingIndex);
    }
    
    private void LoadEndingScene(int endingIndex)
    {
        var mappings = new AssetLabelMapping[]
        {
            new AssetLabelMapping { label = "Ending", assetType = AssetType.EndingSO }
        };
        
        var loadData = CreateLoadingSceneData(mappings, "EndingScene", endingIndex);
        LoadScene("LoadingScene", loadData);
    }
    #endregion

    private IEnumerator WaitForInput() // 입력 대기
    {
        yield return new WaitUntil(() => Input.anyKeyDown && endingCombiner.IsReady);
        LoadEnding();
    }

    public void SetResult()
    {
        text.text = isCleared
            ? "당신은 모든 것을 이뤄내며 빛나는 결말에 도달했습니다."
            : "이 세계에서 당신의 이야기는 끝을 맞이했습니다.";

        float zRotation = isCleared ? 0f : -90f;
        image.rectTransform.rotation = Quaternion.Euler(0, 0, zRotation);
        image.sprite = Manager.Instance.DataManager.CharacterData.Image;

        StartCoroutine(WaitForInput());
    }
}
