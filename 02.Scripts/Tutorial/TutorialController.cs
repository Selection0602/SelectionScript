using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private List<TutorialData> _tutorialSequence;
    [SerializeField] private Canvas _tutorialCanvas;
    
    private TutorialUI _tutorialUI;
    private Dictionary<string, GameObject> _targetCache = new Dictionary<string, GameObject>();
    private bool _tutorialUILoaded = false;
    
    private const string TUTORIAL_UI_PATH = "TutorialUI"; // 어드레서블 주소
    private const string FIRST_MEMORY = "FirstMemory"; // 메모리 튜토리얼 완료 키(값이 있다면 튜토리얼 완료)
    private const string FIRST_BOSS = "FirstBoss"; // 보스 스킬 튜토리얼 완료 키(값이 있다면 튜토리얼 완료)
    
    private Dictionary<TutorialType, string> _completionKeys = new Dictionary<TutorialType, string>(); // 튜토리얼 타입에 따른 완료 키(씬에 따라 키 수정)

    private Action _onTutorialStart;
    private Action _onTutorialComplete;

    private void Awake()
    {
        ServiceLocator.RegisterService(this);
        
        // 현재 씬에 따라 키 수정
        string sceneName = SceneManager.GetActiveScene().name;
        
        foreach (TutorialType type in Enum.GetValues(typeof(TutorialType)))
            _completionKeys[type] = $"Tutorial_{type}_{sceneName}";
    }

    #region 튜토리얼 시작
    /// <summary>
    /// 튜토리얼 시작
    /// </summary>
    /// <param name="type">시작할 튜토리얼 타입</param>
    /// <param name="forceReplay">조건 관계 없이 재생 여부</param>
    private void StartSpecificTutorial(TutorialType type, bool forceReplay = true)
    {
        // 튜토리얼 재생 조건 확인
        string completionKey = _completionKeys[type];
        bool hasCompleted = PlayerPrefs.GetInt(completionKey, 0).Equals(1);
        
        if (!hasCompleted || forceReplay)
        {
            _onTutorialStart?.Invoke();
            
            // 해당 타입의 튜토리얼만 가져오기
            List<TutorialData> filteredSequence = _tutorialSequence
                .Where(data => data.Type == type)
                .ToList();
                
            if (filteredSequence.Count > 0)
                LoadAndShowTutorial(filteredSequence, type);
        }
    }

    /// <summary>
    /// 기본 튜토리얼 시작
    /// </summary>
    /// <param name="tutorialReplay">true = 튜토리얼 조건 관계 없이 재생</param>
    /// <param name="onStart"></param>
    /// <param name="onComplete"></param>
    public void StartTutorial(bool tutorialReplay = false, Action onStart = null, Action onComplete = null)
    {
        _onTutorialStart = onStart;
        _onTutorialComplete = onComplete;

        StartSpecificTutorial(TutorialType.Main, tutorialReplay);
    }
    
    /// <summary>
    /// 메모리 튜토리얼 시작
    /// </summary>
    public void StartMemoryTutorial()
    {
        if (PlayerPrefs.HasKey(FIRST_MEMORY)) return;
        
        StartSpecificTutorial(TutorialType.Memory);
        PlayerPrefs.SetInt(FIRST_MEMORY, 1);
    }
    
    /// <summary>
    /// 보스 스킬 튜토리얼 시작
    /// </summary>
    public void StartBossSkillTutorial()
    {
        if (PlayerPrefs.HasKey(FIRST_BOSS)) return;
        
        StartSpecificTutorial(TutorialType.BossSkill);
        PlayerPrefs.SetInt(FIRST_BOSS, 1);
    }

    /// <summary>
    /// 딜레이 후 튜토리얼 시작
    /// </summary>
    /// <param name="delay">기다릴 시간</param>
    /// <param name="replay">튜토리얼 다시보기 여부</param>
    public void StartTutorialWithDelay(float delay, bool replay)
    {
        StartCoroutine(DelayedTutorialStart(delay, replay));
    }

    private IEnumerator DelayedTutorialStart(float delay, bool replay)
    {
        yield return new WaitForSeconds(delay);
        StartTutorial(replay);
    }
    #endregion

    #region 튜토리얼 로드 및 표시
    private async void LoadAndShowTutorial(List<TutorialData> sequence, TutorialType type)
    {
        try
        {
            CacheTutorialTargets();

            if (!_tutorialUILoaded)
            {
                // 어드레서블 매니저를 통해 프리팹 불러오기
                var tutorialUIObj = await Manager.Instance.AddressableManager.Load<GameObject>(TUTORIAL_UI_PATH);
                if (tutorialUIObj == null)
                    throw new Exception($"어드레서블에서 튜토리얼 오브젝트를 불러오지 못했습니다. {TUTORIAL_UI_PATH}");
                
                GameObject tutorialUI = Instantiate(tutorialUIObj, _tutorialCanvas.transform, false);

                _tutorialUI = tutorialUI.GetComponent<TutorialUI>();
                _tutorialUILoaded = true;
            }

            // 가져온 튜토리얼 UI 초기화 (선택된 튜토리얼 시퀀스와 타입 전달)
            InitializeTutorialUI(sequence, type);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            _tutorialUILoaded = false;
        }
    }

    
    private void CacheTutorialTargets()
    {
        // 씬에 존재하는 튜토리얼 타겟 모두 가져오기 (인스펙터에서 넣어주는걸로 수정예정)
        TutorialTarget[] targets = FindObjectsOfType<TutorialTarget>();
        
        // 튜토리얼 타겟 오브젝트들 딕셔너리에 추가
        foreach (var target in targets)
        {
            if (!string.IsNullOrEmpty(target.TargetId))
                _targetCache[target.TargetId] = target.gameObject;
        }
    }
    
    private void InitializeTutorialUI(List<TutorialData> sequence, TutorialType type)
    {
        if (_tutorialUI != null && sequence != null && sequence.Count > 0)
        {
            _tutorialUI.Initialize(sequence, GetTargetForTutorial, type, _onTutorialComplete);
            _tutorialUI.ShowTutorial(0);
        }
    }
    #endregion

    #region 튜토리얼 타겟 가져오기
    // 매개변수 데이터에 들어있는 타겟들의 오브젝트들 가져오기
    private List<GameObject> GetTargetForTutorial(TutorialData data)
    {
        List<GameObject> targets = new List<GameObject>();
        
        if (data.TargetInfos != null)
        {
            foreach (var targetInfo in data.TargetInfos)
            {
                if (_targetCache.TryGetValue(targetInfo.TargetId, out GameObject target))
                {
                    targets.Add(target);
                }
            }
        }
        
        return targets;
    }
    #endregion

    #region 튜토리얼 완료 처리
    /// <summary>
    /// 현재 씬의 튜토리얼 완료 처리
    /// </summary>
    public void CompleteTutorial(TutorialType type = TutorialType.Main)
    {
        string completionKey = _completionKeys[type];
        PlayerPrefs.SetInt(completionKey, 1);
        PlayerPrefs.Save();
        ReleaseTutorialUI();
    }
    
    private void ReleaseTutorialUI()
    {
        if (_tutorialUI)
        {
            Addressables.ReleaseInstance(_tutorialUI.gameObject);
            _tutorialUI = null;
            _tutorialUILoaded = false;
        }
    }
    
    private void OnDestroy()
    {
        ServiceLocator.UnregisterService<TutorialController>();
        ReleaseTutorialUI();
    }
    #endregion
}
