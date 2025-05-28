using System.Collections;
using UnityEngine;

/// <summary>
/// 탑바(TopBar) 프리팹에 추가하는 스크립트
/// 미니게임이 StartScene에서 시작된 경우 정신력 UI만 비활성화합니다.
/// </summary>
public class StartMiniGameUIController : MonoBehaviour
{
    // 비활성화할 자식 오브젝트 이름 (정신력 UI)
    [SerializeField] private string healthUIName = "Sanity";

    private void Awake()
    {
        // 페이로드 확인을 조금 지연시켜 실행
        StartCoroutine(CheckPayloadWithDelay());
    }
    
    private void OnEnable()
    {        
        // 게임 시작 후 활성화될 때도 체크
        StartCoroutine(CheckPayloadWithDelay());
    }
    
    private IEnumerator CheckPayloadWithDelay()
    {
        // SceneBase가 초기화될 시간을 주기 위해 약간 대기
        yield return new WaitForSeconds(0.1f);
        
        // 현재 씬의 SceneBase 가져오기
        SceneBase currentScene = SceneBase.Current;
        
        if (currentScene == null)
        {
            yield break;
        }
        
        try
        {
            // 페이로드에서 StartScene에서 왔는지 확인
            var payload = currentScene.GetPayload<object>();
            
            if (payload == null)
            {
                yield break;
            }
            
            // 페이로드가 배열인지 확인
            if (payload is object[] payloadArray)
            {
                // 배열 길이 체크 후 fromStartScene 확인
                if (payloadArray.Length > 2 && payloadArray[2] is bool fromStartScene)
                {
                    if (fromStartScene)
                    {
                        // 정신력 UI만 비활성화
                        DisableHealthUI();
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // 예외 처리 (로그 없이)
        }
    }
    
    private void DisableHealthUI()
    {
        // 자식 오브젝트 중 정신력 UI 찾기
        Transform healthUI = transform.Find(healthUIName);
        
        if (healthUI != null)
        {
            healthUI.gameObject.SetActive(false);
        }
    }
} 