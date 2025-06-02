using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class DontMoveManager : MonoBehaviour
{
    [Header("타임라인 PlayableDirector")]
    public PlayableDirector Director;
    [Header("이벤트 시작 버튼")]
    public GameObject PlayBtn;
    [Header("미니게임 스킵 버튼")]
    public GameObject SkipBtn;

    [Header("이벤트 딕셔너리")]
    public Dictionary<int, DontMoveEvent> eventDict = new Dictionary<int, DontMoveEvent>();

    private bool _isPlayed;             //타임라인 실행여부
    private int _prevEventNum = -1;     //이전 이벤트의 고유 번호
    public int CurSuccessCount = 0;        //현재 성공 스택
    public int CurFailedCount = 0;         //현재 실패 스택

    [SerializeField] private int _successCount = 3;        //최종 성공 스택
    [SerializeField] private int _failedCount = 3;         //최종 실패 스택

    [Header("실패 효과 패널")]
    [SerializeField] private GameObject _failEffectPanel;

    [Header("라이프 가시화 Text")]
    [SerializeField] private TextMeshProUGUI _lifeTxt;

    [Header("성공 스택 체크")]
    [SerializeField] private SuccessChecker _successChecker;

    public Coroutine Coroutine;                 //코루틴
    public bool CheatMode = false;              //치트 모드
    public int BookedEventNum = -1;             //예약 이벤트 번호
    public string Label = "DontMoveEvent";      //라벨
    [Header("점프스케어")]
    public JumpScareController JSController;
    [Header("결과 처리")]
    public MiniGameResult MGResult;

    void Start()
    {
        LoadEvents(Label);
        //BGM 재생
        Manager.Instance.SoundManager.PlayBGM(BGMType.BGM_MiniGame);

        Manager.Instance.CreditManager.UnlockMiniGameAndSave(1);

        //점프스케어 이벤트에 씬 이동 등록
        JSController.FailGame.AddListener(() => MGResult.MiniGameManage(false));
        _failEffectPanel.SetActive(false);

        //튜토리얼
        var tutorialController = ServiceLocator.GetService<TutorialController>();
        if (tutorialController)
            tutorialController.StartTutorial();
    }

    #region Addressable
    //이벤트 로드(어드레서블)
    void LoadEvents(string label)
    {
        Addressables.LoadAssetsAsync<GameObject>(label, null).Completed += OnEventsLoaded;
    }

    //이벤트 로드 후 Dictionary에 저장
    void OnEventsLoaded(AsyncOperationHandle<IList<GameObject>> handle)
    {
        //로드 성공시
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (GameObject asset in handle.Result)
            {
                eventDict.Add(asset.gameObject.GetComponent<DontMoveEvent>().EventNumber, asset.gameObject.GetComponent<DontMoveEvent>());
            }
        }
    }
    #endregion

    //게임 시작
    public void StartGame()
    {
        Director = RandomPD(Director);

        Director.stopped += OnTimelineEnd;
        Director.Play();

        _isPlayed = true;
        PlayBtn.gameObject.SetActive(false);
        SkipBtn.gameObject.SetActive(false);
    }

    //다음 랜덤 이벤트 가져오기
    private PlayableDirector RandomPD(PlayableDirector pd)
    {
        PlayableDirector previousPD = pd;

        //처음에 null인 상태를 고려
        if (pd != null)
        {
            Destroy(pd.gameObject);
        }

        //중복이 나오지 않도록 다음 director 선정
        PlayableDirector director = null;
        int randomIndex = -1;

        do
        {
            randomIndex = Random.Range(1, eventDict.Count + 1);
            director = eventDict[randomIndex].Director;
        }
        while (randomIndex == _prevEventNum);

#if UNITY_EDITOR
        //커스텀 에디터로 인한 예약이 없는지 확인
        if (BookedEventNum != -1)
        {
            director = eventDict[BookedEventNum].Director;
            BookedEventNum = -1;
        }
#endif

        //선정된 director를 인스턴스화
        GameObject newDirectorObj = Instantiate(director.gameObject);
        PlayableDirector newDirector = newDirectorObj.GetComponent<PlayableDirector>();

        _prevEventNum = newDirectorObj.gameObject.GetComponent<DontMoveEvent>().EventNumber;
        return newDirector;
    }

    #region Player Input 
    //움직였을 때 감지
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed && _isPlayed && !CheatMode)
        {
            Detect();
        }
    }

    //클릭했을 때 감지
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed && _isPlayed && !CheatMode)
        {
            Detect();
        }
    }

    //클릭했을 때 감지
    public void OnPress(InputAction.CallbackContext context)
    {
        if (context.performed && _isPlayed && !CheatMode)
        {
            Detect();
        }
    }
    #endregion

    //마우스를 감지했을 때 실행하는 함수(해당 이벤트 실패 처리)
    private void Detect()
    {
        Director.Pause();
        Destroy(Director.gameObject);
        CurFailedCount++;

        Coroutine = StartCoroutine(ButtonAppear());
        Coroutine = StartCoroutine(ShowFailEffect());
        _isPlayed = false;
        Director.stopped -= OnTimelineEnd;
        _lifeTxt.text = "Life : " + (3 - CurFailedCount).ToString();

        CheckFinished();
    }

    //타임라인이 다 끝났을 경우 실행되는 함수(해당 이벤트 성공 처리)
    private void OnTimelineEnd(PlayableDirector pd)
    {
        Destroy(Director.gameObject);
        CurSuccessCount++;
        PlayBtn.gameObject.SetActive(true);
        SkipBtn.gameObject.SetActive(true);
        _successChecker.CheckSuccess(CurSuccessCount);

        _isPlayed = false;

        pd.stopped -= OnTimelineEnd;

        CheckFinished();
    }

    //일전 시간 지난 후 버튼 생성
    public IEnumerator ButtonAppear()
    {
        _failEffectPanel.SetActive(true);
        //timer 만들기 
        float curTime = 0f;
        float delayTime = 2f;
        while (delayTime >= curTime)
        {
            curTime += Time.deltaTime;
            yield return null;
        }
        _failEffectPanel.SetActive(false);
        PlayBtn.gameObject.SetActive(true);
        SkipBtn.gameObject.SetActive(true);
    }

    //실패 효과 보여주기
    public IEnumerator ShowFailEffect()
    {
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Damage);
        //패널이 깜뻑거리는 효과
        _failEffectPanel.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        _failEffectPanel.SetActive(false);
        yield return new WaitForSeconds(0.15f);
    }

    //성공과 실패 스택 체크
    void CheckFinished()
    {
        //결과 성공 처리
        if (CurSuccessCount >= _successCount)
        {
            MGResult.MiniGameManage(true);
        }
        else if (CurFailedCount >= _failedCount) JSController.StartJumpScare(0.1f, 0.5f, 2f);     //점프스케어로 이동
        else return;
    }

#if UNITY_EDITOR
    //치트 상태 변경
    public void ChangeCheatMode()
    {
        CheatMode = !CheatMode;
    }

    //성공 스택 한번에 채우기
    public void MakeGameSuccess()
    {
        CurSuccessCount = _successCount;
    }

    //실패 스택 한번에 채우기
    public void MakeGameFailed()
    {
        CurFailedCount = _failedCount;
    }
#endif
}