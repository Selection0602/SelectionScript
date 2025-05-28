using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour
{
    private const int POOL_SIZE = 5;

    private GameObject _effectPrefab;
    private GameObject _textPrefab;
    private GameObject _poolParent;

    private ObjectPool<Effect> _effectPool; // 이펙트 풀
    private ObjectPool<DamageText> _textPool; // 데미지 텍스트 풀
    private Dictionary<EffectType, int> _effectHashDict = new Dictionary<EffectType, int>(); // 이펙트 타입과 해쉬값 매핑 딕셔너리

    private bool _isInitialized = false; // 초기화 여부
    private TaskCompletionSource<bool> _initializeTask = null; // 초기화 Task

    private Canvas _canvas;

    private async void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        _effectPrefab = await Manager.Instance.AddressableManager.Load<GameObject>("EffectPrefab");
        _textPrefab = await Manager.Instance.AddressableManager.Load<GameObject>("DamageTextPrefab");
        EffectData data = await Manager.Instance.AddressableManager.Load<EffectData>("EffectData");
        _effectHashDict = data.EffectDataDict;
    }

    #region 초기화
    private async Task Initialize()
    {
        // 이미 초기화 되었거나 초기화 중인 경우
        if (_initializeTask != null)
        {
            try
            {
                // 초기화 중인 경우 대기
                await _initializeTask.Task;
                return;
            }
            catch
            {
                // 초기화 중 예외 발생 시 다시 초기화 진행
                _initializeTask = null;
            }
        }

        _initializeTask = new TaskCompletionSource<bool>();

        try
        {
            InitializePool();
            await UniTask.Delay(50); // 이펙트 풀 초기화 대기 후 완료 처리
            _initializeTask.TrySetResult(true);
        }
        catch (TaskCanceledException)
        {
            Debug.LogWarning("이펙트 매니저 초기화 실패");
            DestroyPool();
            throw;
        }
    }

    // 캔버스 생성 및 이펙트 풀 생성/초기화
    private void InitializePool()
    {
        if (!_effectPrefab)
        {
            Debug.LogWarning("이펙트 프리팹을 불러오지 못했습니다.");
            return;
        }

        if (!_textPrefab)
        {
            Debug.LogWarning("텍스트 프리팹을 불러오지 못했습니다.");
            return;
        }

        _effectPool = new ObjectPool<Effect>(_effectPrefab);
        _textPool = new ObjectPool<DamageText>(_textPrefab);

        // 캔버스 생성
        GameObject canvasObject = new GameObject("EffectCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        _canvas = canvas;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.sortingOrder = 100;
        canvas.worldCamera = Camera.main;
        LayoutRebuilder.ForceRebuildLayoutImmediate(canvasObject.GetComponent<RectTransform>());

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _canvas.sortingLayerName = "UI";

        // 이펙트 풀 생성 및 초기화
        _poolParent = new GameObject("EffectPool", typeof(RectTransform));
        _poolParent.transform.SetParent(canvas.transform);
        _poolParent.transform.SetAsLastSibling();
        _poolParent.transform.localPosition = Vector3.zero;
        _poolParent.transform.localScale = Vector3.one;

        _effectPool.SetParent(_poolParent.transform);
        _effectPool.InitializePool(POOL_SIZE);

        _textPool.SetParent(_poolParent.transform);
        _textPool.InitializePool(POOL_SIZE);

        _isInitialized = true;
    }
    #endregion

    #region 이펙트 재생 및 텍스트 재생
    /// <summary>
    /// 이펙트 재생
    /// </summary>
    /// <param name="effectType">재생 할 이펙트 타입</param>
    /// <param name="position">이펙트 위치</param>
    /// <param name="onComplete">애니메이션 종료 후 실행 할 액션</param>
    public async Task PlayEffect(EffectType effectType, Vector3 position, Action onComplete = null)
    {
        // 초기화가 진행 중이 아닌 경우 초기화 진행
        if (!_isInitialized || _initializeTask == null)
            await Initialize();
        // 초기화 중인 경우 대기
        else if (!_initializeTask.Task.IsCompleted)
            await _initializeTask.Task;

        // 매개변수로 받은 이펙트 타입의 해쉬값 가져오기
        if (!_effectHashDict.TryGetValue(effectType, out int hash))
        {
            Debug.LogWarning("이펙트가 등록되지 않았습니다.");
            return;
        }

        Effect effect = _effectPool.Get();
        
        // 이펙트의 초기 위치와 크기 설정
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localScale = Vector3.one * 5;
        effect.transform.position = position;
        
        // 이펙트 재생
        effect.PlayEffect(hash);

        // 이펙트가 끝날 때 까지 대기
        await effect.WaitForAnimationComplete();

        onComplete?.Invoke();
        _effectPool.Return(effect);
    }

    /// <summary>
    /// 데미지 텍스트 재생
    /// </summary>
    /// <param name="value">데미지 값 혹은 회복 값</param>
    /// <param name="position">텍스트의 위치</param>
    /// <param name="isHeal">데미지인지 회복인지 (기본값 : 데미지)</param>
    /// <param name="onComplete">텍스트 이동 종료 시 실행 할 함수</param>
    public async Task PlayDamageText(int value, Vector3 position, bool isHeal = false, Action onComplete = null)
    {
        if (!_isInitialized || _initializeTask == null)
            await Initialize();
        else if (!_initializeTask.Task.IsCompleted)
            await _initializeTask.Task;

        DamageText text = _textPool.Get();
        text.transform.localPosition = Vector3.zero;
        text.transform.localScale = Vector3.one;

        // 데미지, 회복 텍스트 설정
        text.SetText((isHeal ? "+" : "-") + value, isHeal ? Color.green : Color.red);
        text.transform.position = position;

        try
        {
            await text.PlayTextEffect();
            onComplete?.Invoke();
        }
        finally
        {
            text.ResetAnimation();
            _textPool.Return(text);
        }
    }
    #endregion

    #region 깜빡거리는 연출

    private int _blinkCount = 2;

    // 데미지를 입었을 때 호출
    public async Task BlinkOnDamage(Image target)
    {
        target.color = Color.red;
        for (int i = 0; i < _blinkCount; i++)
        {
            await target.DOFade(0, 0.05f).AsyncWaitForCompletion();
            await target.DOFade(1, 0.05f).AsyncWaitForCompletion();
        }
        target.color = Color.white;
    }
    #endregion

    private void OnSceneUnloaded(Scene scene)
    {
        if (_isInitialized == true)
            DestroyPool();
    }

    private void DestroyPool()
    {
        if (_poolParent != null)
        {
            Destroy(_poolParent);
            _poolParent = null;
        }

        _isInitialized = false;
        _initializeTask = null;
    }
}
