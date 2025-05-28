using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class DontMoveManager : MonoBehaviour
{
    [Header("이벤트 시작 버튼")]
    public GameObject PlayBtn;
    [Header("미니게임 스킵 버튼")]
    public GameObject SkipBtn;
    [Header("이벤트 로더")]
    public DontMoveEventLoader eventLoader;

    [Header("타임라인 컨트롤러")]
    public DontMoveTimelineController timelineController; //추가

    private bool _isPlayed;             //타임라인 실행여부
    private int _prevEventNum = -1;     //이전 이벤트의 고유 번호
    public int CurSuccessCount = 0;     //현재 성공 스택
    public int CurFailedCount = 0;      //현재 실패 스택

    [SerializeField] private int _successCount = 3;    //최종 성공 스택
    [SerializeField] private int _failedCount = 3;     //최종 실패 스택

    [Header("실패 효과 패널")]
    [SerializeField] private GameObject _failEffectPanel;
    [Header("라이프 가시화 Text")]
    [SerializeField] private TextMeshProUGUI _lifeTxt;
    [Header("성공 스택 체크")]
    [SerializeField] private SuccessChecker _successChecker;

    public Coroutine Coroutine;         //코루틴
    public bool CheatMode = false;      //치트 모드
    public int BookedEventNum = -1;     //예약 이벤트 번호

    [Header("점프스케어")]
    public JumpScareController JSController;
    [Header("결과 처리")]
    public MiniGameResult MGResult;

    void Start()
    {
        if (eventLoader == null)
        {
            //Debug.LogError("DontMoveEventLoader가 할당되지 않았습니다.");
            return;
        }

        eventLoader.OnLoaded += OnEventsLoaded;
        eventLoader.LoadEvents();
        _failEffectPanel.SetActive(false);

        if (timelineController != null)
        {
            timelineController.OnTimelineStopped += OnTimelineEnd;
        }

    }

    private void OnEventsLoaded()
    {
        Manager.Instance.SoundManager.PlayBGM(BGMType.BGM_MiniGame);
        JSController.FailGame.AddListener(() => MGResult.MiniGameManage(false));

        var tutorialController = ServiceLocator.GetService<TutorialController>();
        if (tutorialController != null)
            tutorialController.StartTutorial();

        PlayBtn.SetActive(true);
        SkipBtn.SetActive(true);
    }

    public void StartGame()
    {
        PlayableDirector newDirector = RandomPD(null);
        if (newDirector == null) return;

        timelineController.PlayTimeline(newDirector);

        _isPlayed = true;
        PlayBtn.SetActive(false);
        SkipBtn.SetActive(false);
    }

    private PlayableDirector RandomPD(PlayableDirector pd)
    {
        if (pd != null) Destroy(pd.gameObject);

        PlayableDirector director = null;
        int randomIndex = -1;
        var eventCount = eventLoader.GetEventCount();

        if (eventCount == 0)
        {
            Debug.LogError("로드된 이벤트가 없습니다.");
            return null;
        }

        do
        {
            randomIndex = Random.Range(1, eventCount + 1);
            director = eventLoader.GetEvent(randomIndex)?.Director;
        } while (randomIndex == _prevEventNum || director == null);

#if UNITY_EDITOR
        if (BookedEventNum != -1)
        {
            director = eventLoader.GetEvent(BookedEventNum)?.Director;
            BookedEventNum = -1;
        }
#endif

        GameObject newDirectorObj = Instantiate(director.gameObject);
        PlayableDirector newDirector = newDirectorObj.GetComponent<PlayableDirector>();
        _prevEventNum = newDirectorObj.GetComponent<DontMoveEvent>().EventNumber;
        return newDirector;
    }

    #region Player Input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed && _isPlayed && !CheatMode)
            Detect();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed && _isPlayed && !CheatMode)
            Detect();
    }

    public void OnPress(InputAction.CallbackContext context)
    {
        if (context.performed && _isPlayed && !CheatMode)
            Detect();
    }
    #endregion

    private void Detect()
    {
        if (timelineController == null || !timelineController.IsPlaying) return;

        timelineController.PauseTimeline();
        timelineController.StopTimeline();

        CurFailedCount++;

        Coroutine = StartCoroutine(ButtonAppear());
        Coroutine = StartCoroutine(ShowFailEffect());
        _isPlayed = false;

        _lifeTxt.text = "Life : " + (3 - CurFailedCount).ToString();

        CheckFinished();
    }

    private void OnTimelineEnd()
    {
        CurSuccessCount++;
        PlayBtn.SetActive(true);
        SkipBtn.SetActive(true);
        _successChecker.CheckSuccess(CurSuccessCount);

        _isPlayed = false;

        CheckFinished();
    }

    public IEnumerator ButtonAppear()
    {
        _failEffectPanel.SetActive(true);
        float curTime = 0f;
        float delayTime = 2f;

        while (curTime < delayTime)
        {
            curTime += Time.deltaTime;
            yield return null;
        }

        _failEffectPanel.SetActive(false);
        PlayBtn.SetActive(true);
        SkipBtn.SetActive(true);
    }

    public IEnumerator ShowFailEffect()
    {
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Damage);
        _failEffectPanel.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        _failEffectPanel.SetActive(false);
        yield return new WaitForSeconds(0.15f);
    }

    private void CheckFinished()
    {
        if (CurSuccessCount >= _successCount)
            MGResult.MiniGameManage(true);
        else if (CurFailedCount >= _failedCount)
            JSController.StartJumpScare(0.1f, 0.5f, 2f);
    }

#if UNITY_EDITOR
    public void ChangeCheatMode()
    {
        CheatMode = !CheatMode;
    }

    public void MakeGameSuccess()
    {
        CurSuccessCount = _successCount;
    }

    public void MakeGameFailed()
    {
        CurFailedCount = _failedCount;
    }
#endif
}
