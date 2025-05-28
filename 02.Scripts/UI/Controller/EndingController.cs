using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EndingController : ControllerUI
{
    [SerializeField] private EndingUI endingUI;
    private IEndingView _view;
    private int endingIndex = -1;

    #region ----------------------------- Start -----------------------------
    protected override void OnStart(object data)
    {
        base.OnStart(data);

        // 엔딩 UI 초기화 및 이벤트 설정
        if (endingUI == null)
        {
            return;
        }

        if (data is int idx)
        {
            endingIndex = idx;
        }
        else if (data is object[] arr && arr.Length > 0 && arr[0] is int idx2)
        {
            endingIndex = idx2;
        }
        else
        {
            endingIndex = 1; // 기본 엔딩 인덱스 설정
        }

        StartCoroutine(LoadAndShowEnding(endingIndex));
        Manager.Instance.SaveManager.SaveGame();
    }
    #endregion
    private void Awake()
    {
        _view = endingUI;
    }   
    // EndingSO 로드 및 인덱스에 따라 엔딩 UI 업데이트
    private IEnumerator LoadAndShowEnding(int idx)
    {
        // Return 버튼 초기화 (비활성화)
        _view.InitButton();

        var handle = Addressables.LoadAssetsAsync<EndingSO>("Ending", null);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            EndingSO target = null;
            foreach (var so in handle.Result)
            {
                if (so.Index == idx)
                {
                    target = so;
                    break;
                }
            }
            if (target != null)
            {
                // 엔딩 UI 업데이트 및 애니메이션 시작
                _view.UpdateUI(target);
            }
            else
            {
                // 첫 번째 엔딩이라도 표시
                if (handle.Result.Count > 0)
                {
                    _view.UpdateUI(handle.Result.First());
                }
            }
        }
        Addressables.Release(handle);
    }
}