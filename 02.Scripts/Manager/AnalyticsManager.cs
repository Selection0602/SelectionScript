using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class AnalyticsManager : MonoBehaviour
{
    private string _environment = "production";

    private async void Awake()
    {
#if UNITY_EDITOR
        _environment = "dev";
#else
        _environment = "production";
#endif

        try
        {
            if (FindObjectsOfType<AnalyticsManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            try
            {
                // 애널리틱스 초기화 / 활성화
                var options = new InitializationOptions().SetEnvironmentName(_environment);
                await UnityServices.InitializeAsync(options);
                AnalyticsService.Instance.StartDataCollection();
                Debug.Log($"애널리틱스 초기화 완료, 환경: {_environment}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 이벤트 기록(파라미터 X)
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    public void LogEvent(string eventName)
    {
        try
        {
            AnalyticsService.Instance.RecordEvent(eventName);
            Debug.Log($"이벤트 기록: {eventName}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 이벤트 기록(다중 파라미터)
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <param name="parameters">파라미터들이 들어간 딕셔너리(파라미터 이름 : 파라미터 값)</param>
    public void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        try
        {
            var customEvent = new CustomEvent(eventName);
            foreach (var param in parameters)
            {
                customEvent.Add(param.Key, param.Value);
            }

            AnalyticsService.Instance.RecordEvent(customEvent);
            Debug.Log($"이벤트 기록: {eventName}, 매개변수 개수: {parameters.Count}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 이벤트 등록(단일 파라미터)
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <param name="paramName">파라미터 이름</param>
    /// <param name="paramValue">파라미터 값</param>
    public void LogEvent(string eventName, string paramName, object paramValue)
    {
        try
        {
            var customEvent = new CustomEvent(eventName) { { paramName, paramValue } };
            AnalyticsService.Instance.RecordEvent(customEvent);
            Debug.Log($"이벤트 기록: {eventName}, 매개변수: {paramName}={paramValue}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}